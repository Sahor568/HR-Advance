using Microsoft.EntityFrameworkCore;
using HR_Management_System.Data;
using HR_Management_System.Models;
using HR_Management_System.Models.Enums;
using HR_Management_System.Models.ViewModels;
using HR_Management_System.Services.Interfaces;

namespace HR_Management_System.Services.Implementations
{
    public class PayrollService : IPayrollService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PayrollService> _logger;

        public PayrollService(ApplicationDbContext context, ILogger<PayrollService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Payroll> GeneratePayrollAsync(int employeeId, int month, int year, bool includeFestivalAllowance = false)
        {
            _logger.LogInformation("Generating payroll for employee ID: {EmployeeId}, {Month}/{Year}", employeeId, month, year);

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
                throw new KeyNotFoundException("Employee not found");

            // Check if payroll already exists
            var existingPayroll = await _context.Payrolls
                .FirstOrDefaultAsync(p => p.EmployeeId == employeeId && p.Month == month && p.Year == year);

            if (existingPayroll != null)
            {
                _logger.LogWarning("Payroll already exists for employee {EmployeeId}, {Month}/{Year}", employeeId, month, year);
                return existingPayroll;
            }

            // Calculate earnings
            var baseSalary = employee.BaseSalary;
            var gradeAmount = employee.GradeAmount;

            // Calculate overtime
            var overtimeHours = await GetOvertimeHoursAsync(employeeId, month, year);
            var hourlyRate = baseSalary / 208; // Assuming 8 hours/day * 26 days/month = 208 hours
            var overtimeAmount = await CalculateOvertimePayAsync(baseSalary, overtimeHours);

            // Calculate festival allowance (Dashain bonus = 1 month salary)
            var festivalAllowance = includeFestivalAllowance ? baseSalary : 0;

            // Calculate SSF contributions
            var ssfEmployeeContribution = baseSalary * 0.10m; // 10% employee
            var ssfEmployerContribution = baseSalary * 0.21m; // 21% employer

            // Calculate TDS
            var annualTaxableIncome = await CalculateAnnualTaxableIncomeAsync(employeeId, year);
            var tds = await CalculateTDSCalculationAsync(annualTaxableIncome / 12); // Monthly TDS

            // Calculate working days
            var workingDays = await GetWorkingDaysAsync(month, year);
            var daysWorked = await GetDaysWorkedAsync(employeeId, month, year);
            var leaveDays = await GetLeaveDaysAsync(employeeId, month, year);

            var payroll = new Payroll
            {
                EmployeeId = employeeId,
                Month = month,
                Year = year,
                Base_Salary = baseSalary,
                Grade_Amount = gradeAmount,
                OvertimeAmount = overtimeAmount,
                FestivalAllowance = festivalAllowance,
                OtherAllowances = 0,
                SSF_Employee_Contribution = ssfEmployeeContribution,
                SSF_Employer_Contribution = ssfEmployerContribution,
                Tax_Deduction = tds,
                OtherDeductions = 0,
                OvertimeHours = overtimeHours,
                HourlyRate = hourlyRate,
                WorkingDays = workingDays,
                DaysWorked = daysWorked,
                LeaveDays = leaveDays,
                AbsentDays = workingDays - daysWorked - leaveDays,
                TaxableIncome = baseSalary + gradeAmount + overtimeAmount + festivalAllowance,
                AnnualTaxableIncome = annualTaxableIncome,
                TaxRate = await GetTaxRateAsync(annualTaxableIncome),
                Status = PayrollStatus.Generated,
                CreatedAt = DateTime.UtcNow
            };

            _context.Payrolls.Add(payroll);
            await _context.SaveChangesAsync();

            // Generate SSF record
            await GenerateSSFRecordAsync(employeeId, month, year);

            _logger.LogInformation("Payroll generated successfully. Payroll ID: {PayrollId}", payroll.Id);
            return payroll;
        }

        public async Task<IEnumerable<Payroll>> GenerateBulkPayrollAsync(int month, int year, bool includeFestivalAllowance = false)
        {
            _logger.LogInformation("Generating bulk payroll for {Month}/{Year}", month, year);

            var activeEmployees = await _context.Employees
                .Where(e => e.IsActive)
                .Select(e => e.Id)
                .ToListAsync();

            var payrolls = new List<Payroll>();
            foreach (var employeeId in activeEmployees)
            {
                try
                {
                    var payroll = await GeneratePayrollAsync(employeeId, month, year, includeFestivalAllowance);
                    payrolls.Add(payroll);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating payroll for employee ID: {EmployeeId}", employeeId);
                }
            }

            _logger.LogInformation("Bulk payroll generation completed. Generated {Count} payrolls", payrolls.Count);
            return payrolls;
        }

        public async Task<bool> ApprovePayrollAsync(int payrollId, int approvedBy)
        {
            _logger.LogInformation("Approving payroll ID: {PayrollId}", payrollId);

            var payroll = await _context.Payrolls.FindAsync(payrollId);
            if (payroll == null) return false;

            payroll.Status = PayrollStatus.Approved;
            payroll.ApprovedBy = approvedBy;
            payroll.ApprovedDate = DateTime.UtcNow;
            payroll.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Payroll {PayrollId} approved successfully", payrollId);
            return true;
        }

        public async Task<bool> MarkAsPaidAsync(int payrollId)
        {
            _logger.LogInformation("Marking payroll ID: {PayrollId} as paid", payrollId);

            var payroll = await _context.Payrolls.FindAsync(payrollId);
            if (payroll == null) return false;

            if (payroll.Status != PayrollStatus.Approved)
            {
                _logger.LogWarning("Payroll {PayrollId} must be approved before marking as paid", payrollId);
                return false;
            }

            payroll.Status = PayrollStatus.Paid;
            payroll.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Payroll {PayrollId} marked as paid", payrollId);
            return true;
        }

        public async Task<PayslipViewModel> GetPayslipAsync(int payrollId)
        {
            _logger.LogInformation("Fetching payslip for payroll ID: {PayrollId}", payrollId);

            var payroll = await _context.Payrolls
                .Include(p => p.Employee)
                .FirstOrDefaultAsync(p => p.Id == payrollId);

            if (payroll == null)
                throw new KeyNotFoundException("Payroll not found");

            return new PayslipViewModel
            {
                PayrollId = payroll.Id,
                Emp_ID = payroll.Employee?.Emp_ID ?? "",
                EmployeeName = payroll.Employee?.FullName ?? "",
                Designation = payroll.Employee?.Designation ?? "",
                Department = payroll.Employee?.Department ?? "",
                Month = payroll.Month,
                Year = payroll.Year,
                BaseSalary = payroll.Base_Salary,
                GradeAmount = payroll.Grade_Amount,
                OvertimeAmount = payroll.OvertimeAmount,
                FestivalAllowance = payroll.FestivalAllowance,
                OtherAllowances = payroll.OtherAllowances,
                GrossEarnings = payroll.GrossEarnings,
                SSFEmployeeContribution = payroll.SSF_Employee_Contribution,
                TaxDeduction = payroll.Tax_Deduction,
                OtherDeductions = payroll.OtherDeductions,
                TotalDeductions = payroll.TotalDeductions,
                NetPay = payroll.Net_Pay,
                WorkingDays = payroll.WorkingDays,
                DaysWorked = payroll.DaysWorked,
                OvertimeHours = payroll.OvertimeHours,
                HourlyRate = payroll.HourlyRate,
                PayrollStatus = payroll.Status.ToString()
            };
        }

        public async Task<IEnumerable<PayslipViewModel>> GetEmployeePayslipsAsync(int employeeId)
        {
            _logger.LogInformation("Fetching payslips for employee ID: {EmployeeId}", employeeId);

            return await _context.Payrolls
                .Include(p => p.Employee)
                .Where(p => p.EmployeeId == employeeId)
                .OrderByDescending(p => p.Year)
                .ThenByDescending(p => p.Month)
                .Select(p => new PayslipViewModel
                {
                    PayrollId = p.Id,
                    Emp_ID = p.Employee!.Emp_ID,
                    EmployeeName = p.Employee.FullName,
                    Designation = p.Employee.Designation,
                    Department = p.Employee.Department,
                    Month = p.Month,
                    Year = p.Year,
                    BaseSalary = p.Base_Salary,
                    GradeAmount = p.Grade_Amount,
                    OvertimeAmount = p.OvertimeAmount,
                    FestivalAllowance = p.FestivalAllowance,
                    OtherAllowances = p.OtherAllowances,
                    GrossEarnings = p.GrossEarnings,
                    SSFEmployeeContribution = p.SSF_Employee_Contribution,
                    TaxDeduction = p.Tax_Deduction,
                    OtherDeductions = p.OtherDeductions,
                    TotalDeductions = p.TotalDeductions,
                    NetPay = p.Net_Pay,
                    WorkingDays = p.WorkingDays,
                    DaysWorked = p.DaysWorked,
                    OvertimeHours = p.OvertimeHours,
                    HourlyRate = p.HourlyRate,
                    PayrollStatus = p.Status.ToString()
                })
                .ToListAsync();
        }

        public async Task<decimal> CalculateTDSCalculationAsync(decimal annualTaxableIncome)
        {
            // Nepal Income Tax Slabs (2024)
            if (annualTaxableIncome <= 500000) return 0; // No tax up to 5 lakhs
            if (annualTaxableIncome <= 700000) return (annualTaxableIncome - 500000) * 0.10m / 12;
            if (annualTaxableIncome <= 1000000) return (200000 * 0.10m + (annualTaxableIncome - 700000) * 0.20m) / 12;
            if (annualTaxableIncome <= 2000000) return (200000 * 0.10m + 300000 * 0.20m + (annualTaxableIncome - 1000000) * 0.30m) / 12;
            return (200000 * 0.10m + 300000 * 0.20m + 1000000 * 0.30m + (annualTaxableIncome - 2000000) * 0.36m) / 12;
        }

        public async Task<decimal> CalculateOvertimePayAsync(decimal baseSalary, decimal overtimeHours)
        {
            var hourlyRate = baseSalary / 208; // 8 hours/day * 26 days/month
            return overtimeHours * hourlyRate * 1.5m; // 1.5x hourly wage for extra hours
        }

        public async Task<SSFRecord> GenerateSSFRecordAsync(int employeeId, int month, int year)
        {
            _logger.LogInformation("Generating SSF record for employee ID: {EmployeeId}, {Month}/{Year}", employeeId, month, year);

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
                throw new KeyNotFoundException("Employee not found");

            var existingRecord = await _context.SSFRecords
                .FirstOrDefaultAsync(s => s.EmployeeId == employeeId && s.Month == month && s.Year == year);

            if (existingRecord != null)
            {
                _logger.LogWarning("SSF record already exists for employee {EmployeeId}, {Month}/{Year}", employeeId, month, year);
                return existingRecord;
            }

            var baseSalary = employee.BaseSalary;
            var employeeContribution = baseSalary * 0.10m; // 10% employee
            var employerContribution = baseSalary * 0.21m; // 21% employer

            // Get previous balance
            var previousRecord = await _context.SSFRecords
                .Where(s => s.EmployeeId == employeeId)
                .OrderByDescending(s => s.Year).ThenByDescending(s => s.Month)
                .FirstOrDefaultAsync();

            var totalEmployeeBalance = (previousRecord?.TotalEmployeeBalance ?? 0) + employeeContribution;
            var totalEmployerBalance = (previousRecord?.TotalEmployerBalance ?? 0) + employerContribution;

            var ssfRecord = new SSFRecord
            {
                EmployeeId = employeeId,
                Month = month,
                Year = year,
                EmployeeContribution = employeeContribution,
                EmployerContribution = employerContribution,
                ProvidentFundEmployer = employerContribution * 0.60m, // 60% for provident fund
                GratuityEmployer = employerContribution * 0.25m, // 25% for gratuity
                SSFEmployer = employerContribution * 0.15m, // 15% for SSF
                TotalEmployeeBalance = totalEmployeeBalance,
                TotalEmployerBalance = totalEmployerBalance,
                BaseSalary = baseSalary,
                CreatedAt = DateTime.UtcNow
            };

            _context.SSFRecords.Add(ssfRecord);
            await _context.SaveChangesAsync();
            _logger.LogInformation("SSF record generated successfully. Record ID: {RecordId}", ssfRecord.Id);
            return ssfRecord;
        }

        public async Task<IEnumerable<Payroll>> GetPayrollsByMonthYearAsync(int month, int year)
        {
            return await _context.Payrolls
                .Include(p => p.Employee)
                .Where(p => p.Month == month && p.Year == year)
                .OrderBy(p => p.EmployeeId)
                .ToListAsync();
        }

        public async Task<int> GetPendingPayrollCountAsync()
        {
            return await _context.Payrolls
                .CountAsync(p => p.Status == PayrollStatus.Draft || p.Status == PayrollStatus.Generated);
        }

        public async Task<decimal> GetTotalMonthlyPayrollAsync(int month, int year)
        {
            return await _context.Payrolls
                .Where(p => p.Month == month && p.Year == year && p.Status == PayrollStatus.Paid)
                .SumAsync(p => p.Net_Pay);
        }

        private async Task<decimal> GetOvertimeHoursAsync(int employeeId, int month, int year)
        {
            return await _context.Attendances
                .Where(a => a.EmployeeId == employeeId && a.Date.Month == month && a.Date.Year == year)
                .SumAsync(a => a.OT_Hours);
        }

        private async Task<decimal> CalculateAnnualTaxableIncomeAsync(int employeeId, int year)
        {
            var payrolls = await _context.Payrolls
                .Where(p => p.EmployeeId == employeeId && p.Year == year)
                .ToListAsync();

            return payrolls.Sum(p => p.TaxableIncome);
        }

        private async Task<decimal> GetWorkingDaysAsync(int month, int year)
        {
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var workingDays = 0;

            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(year, month, day);
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                {
                    // Check if it's a public holiday
                    var isHoliday = await _context.PublicHolidays
                        .AnyAsync(ph => ph.HolidayDate.Date == date.Date && ph.IsActive);

                    if (!isHoliday)
                        workingDays++;
                }
            }

            return workingDays;
        }

        private async Task<decimal> GetDaysWorkedAsync(int employeeId, int month, int year)
        {
            return await _context.Attendances
                .CountAsync(a => a.EmployeeId == employeeId && a.Date.Month == month && a.Date.Year == year && a.Status == AttendanceStatus.Present);
        }

        private async Task<decimal> GetLeaveDaysAsync(int employeeId, int month, int year)
        {
            return await _context.LeaveRequests
                .Where(lr => lr.EmployeeId == employeeId &&
                            lr.Status == LeaveStatus.Approved &&
                            lr.StartDate.Month == month && lr.StartDate.Year == year)
                .SumAsync(lr => lr.NumberOfDays);
        }

        private async Task<decimal> GetTaxRateAsync(decimal annualTaxableIncome)
        {
            if (annualTaxableIncome <= 500000) return 0;
            if (annualTaxableIncome <= 700000) return 10;
            if (annualTaxableIncome <= 1000000) return 20;
            if (annualTaxableIncome <= 2000000) return 30;
            return 36;
        }
    }
}