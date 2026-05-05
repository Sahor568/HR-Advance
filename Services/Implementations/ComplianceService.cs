using Microsoft.EntityFrameworkCore;
using HR_Management_System.Data;
using HR_Management_System.Models;
using HR_Management_System.Models.Enums;
using HR_Management_System.Models.ViewModels;
using HR_Management_System.Services.Interfaces;

namespace HR_Management_System.Services.Implementations
{
    public class ComplianceService : IComplianceService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ComplianceService> _logger;

        public ComplianceService(ApplicationDbContext context, ILogger<ComplianceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<LaborAuditReport> GenerateLaborAuditReportAsync(AuditReportGenerateViewModel model, int generatedBy)
        {
            _logger.LogInformation("Generating labor audit report for period {Start} to {End}", model.PeriodStart, model.PeriodEnd);

            var reportNumber = $"AUD-{DateTime.UtcNow:yyyyMMddHHmmss}";

            // Calculate compliance metrics
            var totalEmployees = await _context.Employees.CountAsync();
            var activeEmployees = await _context.Employees.CountAsync(e => e.IsActive);
            var employeesOnProbation = await _context.Employees.CountAsync(e => e.IsActive && e.ProbationStatus == ProbationStatus.Active);

            // Attendance compliance
            var averageWorkingHours = await _context.Attendances
                .Where(a => a.Date >= model.PeriodStart && a.Date <= model.PeriodEnd && a.Status == AttendanceStatus.Present)
                .AverageAsync(a => a.TotalHours);

            var overtimeViolations = await _context.Attendances
                .Where(a => a.Date >= model.PeriodStart && a.Date <= model.PeriodEnd && a.OT_Hours > 4) // More than 4 hours OT per day
                .CountAsync();

            var workHourViolations = await GetWeeklyWorkHourViolationsAsync(model.PeriodStart, model.PeriodEnd);

            // Leave compliance
            var leaveViolations = await _context.LeaveRequests
                .Where(lr => lr.StartDate >= model.PeriodStart && lr.EndDate <= model.PeriodEnd && lr.Status == LeaveStatus.Rejected)
                .CountAsync();

            var maternityLeaveGranted = await _context.LeaveRequests
                .CountAsync(lr => lr.LeaveType == LeaveTypeEnum.Maternity && lr.Status == LeaveStatus.Approved &&
                                 lr.StartDate >= model.PeriodStart && lr.EndDate <= model.PeriodEnd);

            var paternityLeaveGranted = await _context.LeaveRequests
                .CountAsync(lr => lr.LeaveType == LeaveTypeEnum.Paternity && lr.Status == LeaveStatus.Approved &&
                                 lr.StartDate >= model.PeriodStart && lr.EndDate <= model.PeriodEnd);

            // Payroll compliance
            var payrolls = await _context.Payrolls
                .Where(p => p.CreatedAt >= model.PeriodStart && p.CreatedAt <= model.PeriodEnd)
                .ToListAsync();

            var totalPayrollAmount = payrolls.Sum(p => p.Net_Pay);
            var totalTaxDeducted = payrolls.Sum(p => p.Tax_Deduction);
            var totalSSFContribution = payrolls.Sum(p => p.SSF_Employee_Contribution + p.SSF_Employer_Contribution);
            var payrollDiscrepancies = payrolls.Count(p => p.Status != PayrollStatus.Paid);

            // OHS
            var totalAccidents = await _context.AccidentLogs
                .CountAsync(a => a.AccidentDate >= model.PeriodStart && a.AccidentDate <= model.PeriodEnd);

            var unreportedAccidents = await _context.AccidentLogs
                .CountAsync(a => a.AccidentDate >= model.PeriodStart && a.AccidentDate <= model.PeriodEnd && !a.ReportedToAuthorities);

            var pendingInsuranceClaims = await _context.MedicalInsuranceClaims
                .CountAsync(c => c.ClaimDate >= model.PeriodStart && c.ClaimDate <= model.PeriodEnd && c.Status == ClaimStatus.Pending);

            // Disciplinary
            var totalDisciplinaryActions = await _context.DisciplinaryRecords
                .CountAsync(d => d.IncidentDate >= model.PeriodStart && d.IncidentDate <= model.PeriodEnd);

            var section131Cases = await _context.DisciplinaryRecords
                .CountAsync(d => d.IncidentDate >= model.PeriodStart && d.IncidentDate <= model.PeriodEnd && d.IsSection131Compliance);

            var unresolvedCases = await _context.DisciplinaryRecords
                .CountAsync(d => d.IncidentDate >= model.PeriodStart && d.IncidentDate <= model.PeriodEnd && !d.IsResolved);

            // Festival Bonus
            var festivalBonusPaid = payrolls.Count(p => p.FestivalAllowance > 0);
            var festivalBonusPending = activeEmployees - festivalBonusPaid;

            var report = new LaborAuditReport
            {
                ReportNumber = reportNumber,
                ReportType = (ReportType)model.ReportType,
                PeriodStart = model.PeriodStart,
                PeriodEnd = model.PeriodEnd,
                GeneratedDate = DateTime.UtcNow,
                GeneratedBy = generatedBy,
                TotalEmployees = totalEmployees,
                ActiveEmployees = activeEmployees,
                EmployeesOnProbation = employeesOnProbation,
                AverageWorkingHours = averageWorkingHours,
                OvertimeViolations = overtimeViolations,
                WorkHourViolations = workHourViolations,
                LeaveViolations = leaveViolations,
                MaternityLeaveGranted = maternityLeaveGranted,
                PaternityLeaveGranted = paternityLeaveGranted,
                TotalPayrollAmount = totalPayrollAmount,
                TotalTaxDeducted = totalTaxDeducted,
                TotalSSFContribution = totalSSFContribution,
                PayrollDiscrepancies = payrollDiscrepancies,
                TotalAccidents = totalAccidents,
                UnreportedAccidents = unreportedAccidents,
                PendingInsuranceClaims = pendingInsuranceClaims,
                TotalDisciplinaryActions = totalDisciplinaryActions,
                Section131Cases = section131Cases,
                UnresolvedCases = unresolvedCases,
                FestivalBonusPaid = festivalBonusPaid,
                FestivalBonusPending = festivalBonusPending,
                Remarks = model.Remarks,
                CreatedAt = DateTime.UtcNow
            };

            _context.LaborAuditReports.Add(report);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Labor audit report generated successfully. Report Number: {ReportNumber}", reportNumber);
            return report;
        }

        public async Task<AccidentLog> LogAccidentAsync(AccidentLogCreateViewModel model)
        {
            _logger.LogInformation("Logging accident for employee ID: {EmployeeId}", model.EmployeeId);

            var accidentLog = new AccidentLog
            {
                EmployeeId = model.EmployeeId,
                AccidentDate = model.AccidentDate,
                AccidentTime = model.AccidentTime,
                Location = model.Location,
                Severity = (AccidentSeverity)model.Severity,
                Description = model.Description,
                ImmediateActionTaken = model.ImmediateActionTaken,
                RootCause = model.RootCause,
                PreventiveMeasures = model.PreventiveMeasures,
                WitnessNames = model.WitnessNames,
                InjuryDetails = model.InjuryDetails,
                RequiredHospitalization = model.RequiredHospitalization,
                ReportedToAuthorities = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.AccidentLogs.Add(accidentLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Accident logged successfully. Accident ID: {AccidentId}", accidentLog.Id);
            return accidentLog;
        }

        public async Task<AccidentLog> UpdateAccidentLogAsync(AccidentLog log)
        {
            _logger.LogInformation("Updating accident log ID: {AccidentId}", log.Id);

            log.UpdatedAt = DateTime.UtcNow;
            _context.AccidentLogs.Update(log);
            await _context.SaveChangesAsync();

            return log;
        }

        public async Task<IEnumerable<AccidentLog>> GetAccidentLogsAsync(DateTime? from = null, DateTime? to = null)
        {
            var query = _context.AccidentLogs
                .Include(a => a.Employee)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(a => a.AccidentDate >= from.Value);

            if (to.HasValue)
                query = query.Where(a => a.AccidentDate <= to.Value);

            return await query
                .OrderByDescending(a => a.AccidentDate)
                .ThenByDescending(a => a.AccidentTime)
                .ToListAsync();
        }

        public async Task<MedicalInsuranceClaim> SubmitInsuranceClaimAsync(MedicalClaimCreateViewModel model)
        {
            _logger.LogInformation("Submitting insurance claim for employee ID: {EmployeeId}", model.EmployeeId);

            var claimNumber = $"CLM-{DateTime.UtcNow:yyyyMMddHHmmss}";

            var claim = new MedicalInsuranceClaim
            {
                EmployeeId = model.EmployeeId,
                ClaimNumber = claimNumber,
                ClaimDate = DateTime.UtcNow,
                ClaimType = model.ClaimType,
                ClaimAmount = model.ClaimAmount,
                ApprovedAmount = 0,
                Status = ClaimStatus.Pending,
                Description = model.Description,
                HospitalDetails = model.HospitalDetails,
                TreatmentStartDate = model.TreatmentStartDate,
                TreatmentEndDate = model.TreatmentEndDate,
                CreatedAt = DateTime.UtcNow
            };

            _context.MedicalInsuranceClaims.Add(claim);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Insurance claim submitted successfully. Claim Number: {ClaimNumber}", claimNumber);
            return claim;
        }

        public async Task<bool> ApproveInsuranceClaimAsync(int claimId, int approvedBy, decimal approvedAmount, string remarks)
        {
            _logger.LogInformation("Approving insurance claim ID: {ClaimId}", claimId);

            var claim = await _context.MedicalInsuranceClaims.FindAsync(claimId);
            if (claim == null) return false;

            // Check coverage limits
            if (claim.ClaimType == "Medical" && approvedAmount > claim.MedicalCoverageLimit)
            {
                _logger.LogWarning("Approved amount {ApprovedAmount} exceeds medical coverage limit {Limit}", approvedAmount, claim.MedicalCoverageLimit);
                return false;
            }

            if (claim.ClaimType == "Accident" && approvedAmount > claim.AccidentCoverageLimit)
            {
                _logger.LogWarning("Approved amount {ApprovedAmount} exceeds accident coverage limit {Limit}", approvedAmount, claim.AccidentCoverageLimit);
                return false;
            }

            claim.Status = ClaimStatus.Approved;
            claim.ApprovedAmount = approvedAmount;
            claim.ApprovedBy = approvedBy;
            claim.ApprovedDate = DateTime.UtcNow;
            claim.ApprovalRemarks = remarks;
            claim.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Insurance claim {ClaimId} approved successfully", claimId);
            return true;
        }

        public async Task<bool> RejectInsuranceClaimAsync(int claimId, int rejectedBy, string reason)
        {
            _logger.LogInformation("Rejecting insurance claim ID: {ClaimId}", claimId);

            var claim = await _context.MedicalInsuranceClaims.FindAsync(claimId);
            if (claim == null) return false;

            claim.Status = ClaimStatus.Rejected;
            claim.RejectionReason = reason;
            claim.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Insurance claim {ClaimId} rejected", claimId);
            return true;
        }

        public async Task<IEnumerable<MedicalInsuranceClaim>> GetPendingClaimsAsync()
        {
            return await _context.MedicalInsuranceClaims
                .Include(c => c.Employee)
                .Where(c => c.Status == ClaimStatus.Pending)
                .OrderByDescending(c => c.ClaimDate)
                .ToListAsync();
        }

        public async Task<DisciplinaryRecord> CreateDisciplinaryRecordAsync(DisciplinaryCreateViewModel model, int issuedBy)
        {
            _logger.LogInformation("Creating disciplinary record for employee ID: {EmployeeId}", model.EmployeeId);

            var record = new DisciplinaryRecord
            {
                EmployeeId = model.EmployeeId,
                ActionType = (DisciplinaryActionType)model.ActionType,
                IncidentDate = model.IncidentDate,
                Description = model.Description,
                IsSection131Compliance = model.IsSection131Compliance,
                Section131Details = model.Section131Details,
                WarningDetails = model.WarningDetails,
                IssuedBy = issuedBy,
                IssuedDate = DateTime.UtcNow,
                SuspensionStartDate = model.SuspensionStartDate,
                SuspensionEndDate = model.SuspensionEndDate,
                IsResolved = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.DisciplinaryRecords.Add(record);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Disciplinary record created successfully. Record ID: {RecordId}", record.Id);
            return record;
        }

        public async Task<bool> ResolveDisciplinaryCaseAsync(int recordId, string resolution)
        {
            _logger.LogInformation("Resolving disciplinary case ID: {RecordId}", recordId);

            var record = await _context.DisciplinaryRecords.FindAsync(recordId);
            if (record == null) return false;

            record.IsResolved = true;
            record.Resolution = resolution;
            record.ResolutionDate = DateTime.UtcNow;
            record.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Disciplinary case {RecordId} resolved", recordId);
            return true;
        }

        public async Task<IEnumerable<DisciplinaryRecord>> GetUnresolvedDisciplinaryCasesAsync()
        {
            return await _context.DisciplinaryRecords
                .Include(d => d.Employee)
                .Where(d => !d.IsResolved)
                .OrderByDescending(d => d.IncidentDate)
                .ToListAsync();
        }

        public async Task<int> GetOpenAccidentCountAsync()
        {
            return await _context.AccidentLogs
                .CountAsync(a => !a.ReportedToAuthorities);
        }

        public async Task<int> GetPendingInsuranceClaimCountAsync()
        {
            return await _context.MedicalInsuranceClaims
                .CountAsync(c => c.Status == ClaimStatus.Pending);
        }

        public async Task<int> GetUnresolvedDisciplinaryCountAsync()
        {
            return await _context.DisciplinaryRecords
                .CountAsync(d => !d.IsResolved);
        }

        private async Task<int> GetWeeklyWorkHourViolationsAsync(DateTime periodStart, DateTime periodEnd)
        {
            var violations = 0;
            var employees = await _context.Employees
                .Where(e => e.IsActive)
                .Select(e => e.Id)
                .ToListAsync();

            foreach (var employeeId in employees)
            {
                var weeklyHours = await GetWeeklyHoursForEmployeeAsync(employeeId, periodStart, periodEnd);
                if (weeklyHours.Any(wh => wh > 48))
                    violations++;
            }

            return violations;
        }

        private async Task<List<decimal>> GetWeeklyHoursForEmployeeAsync(int employeeId, DateTime periodStart, DateTime periodEnd)
        {
            var attendances = await _context.Attendances
                .Where(a => a.EmployeeId == employeeId && a.Date >= periodStart && a.Date <= periodEnd && a.Status == AttendanceStatus.Present)
                .ToListAsync();

            var weeklyHours = new List<decimal>();
            var currentWeekStart = periodStart.StartOfWeek(DayOfWeek.Sunday);

            while (currentWeekStart <= periodEnd)
            {
                var weekEnd = currentWeekStart.AddDays(6);
                var weekHours = attendances
                    .Where(a => a.Date >= currentWeekStart && a.Date <= weekEnd)
                    .Sum(a => a.TotalHours);

                weeklyHours.Add(weekHours);
                currentWeekStart = currentWeekStart.AddDays(7);
            }

            return weeklyHours;
        }
    }
}