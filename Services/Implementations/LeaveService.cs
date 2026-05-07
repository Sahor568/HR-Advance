using Microsoft.EntityFrameworkCore;
using HR_Management_System.Data;
using HR_Management_System.Models;
using HR_Management_System.Models.Enums;
using HR_Management_System.Models.ViewModels;
using HR_Management_System.Services.Interfaces;

namespace HR_Management_System.Services.Implementations
{
    public class LeaveService : ILeaveService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LeaveService> _logger;

        public LeaveService(ApplicationDbContext context, ILogger<LeaveService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<LeaveRequest> SubmitLeaveRequestAsync(LeaveRequestViewModel model)
        {
            _logger.LogInformation("Submitting leave request for employee ID: {EmployeeId}, Type: {LeaveType}", model.EmployeeId, model.LeaveType);

            var employee = await _context.Employees.FindAsync(model.EmployeeId);
            if (employee == null)
                throw new KeyNotFoundException("Employee not found");

            // Calculate number of days
            var numberOfDays = (decimal)(model.EndDate - model.StartDate).TotalDays + 1;

            // Validate leave balance
            var leaveBalance = await GetLeaveBalanceAsync(model.EmployeeId, model.StartDate.Year);
            var leaveType = (LeaveTypeEnum)model.LeaveType;

            await ValidateLeaveBalance(leaveBalance, leaveType, numberOfDays, employee);

            var leaveRequest = new LeaveRequest
            {
                EmployeeId = model.EmployeeId,
                LeaveType = leaveType,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                NumberOfDays = numberOfDays,
                Reason = model.Reason,
                DocumentPath = model.DocumentPath,
                Status = LeaveStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.LeaveRequests.Add(leaveRequest);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Leave request submitted successfully. Request ID: {RequestId}", leaveRequest.Id);
            return leaveRequest;
        }

        public async Task<bool> ApproveLeaveRequestAsync(LeaveApprovalViewModel model, int approvedBy)
        {
            _logger.LogInformation("Approving leave request ID: {RequestId}", model.LeaveRequestId);

            var leaveRequest = await _context.LeaveRequests.FindAsync(model.LeaveRequestId);
            if (leaveRequest == null) return false;

            if (leaveRequest.Status != LeaveStatus.Pending)
            {
                _logger.LogWarning("Leave request {RequestId} is not in pending status", model.LeaveRequestId);
                return false;
            }

            leaveRequest.Status = LeaveStatus.Approved;
            leaveRequest.ApprovedBy = approvedBy;
            leaveRequest.ApprovedDate = DateTime.UtcNow;
            leaveRequest.ApprovalRemarks = model.Remarks;
            leaveRequest.UpdatedAt = DateTime.UtcNow;

            // Update leave balance
            var leaveBalance = await _context.LeaveBalances
                .FirstOrDefaultAsync(lb => lb.EmployeeId == leaveRequest.EmployeeId && lb.Year == leaveRequest.StartDate.Year);

            if (leaveBalance != null)
            {
                UpdateLeaveBalanceUsed(leaveBalance, leaveRequest.LeaveType, leaveRequest.NumberOfDays);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Leave request {RequestId} approved successfully", model.LeaveRequestId);
            return true;
        }

        public async Task<bool> RejectLeaveRequestAsync(int leaveRequestId, int rejectedBy, string reason)
        {
            _logger.LogInformation("Rejecting leave request ID: {RequestId}", leaveRequestId);

            var leaveRequest = await _context.LeaveRequests.FindAsync(leaveRequestId);
            if (leaveRequest == null) return false;

            if (leaveRequest.Status != LeaveStatus.Pending) return false;

            leaveRequest.Status = LeaveStatus.Rejected;
            leaveRequest.RejectedBy = rejectedBy;
            leaveRequest.RejectedDate = DateTime.UtcNow;
            leaveRequest.RejectionReason = reason;
            leaveRequest.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Leave request {RequestId} rejected", leaveRequestId);
            return true;
        }

        public async Task<IEnumerable<LeaveRequest>> GetPendingLeaveRequestsAsync()
        {
            return await _context.LeaveRequests
                .Include(lr => lr.Employee)
                .Where(lr => lr.Status == LeaveStatus.Pending)
                .OrderByDescending(lr => lr.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<LeaveRequest>> GetEmployeeLeaveRequestsAsync(int employeeId)
        {
            return await _context.LeaveRequests
                .Where(lr => lr.EmployeeId == employeeId)
                .OrderByDescending(lr => lr.CreatedAt)
                .ToListAsync();
        }

        public async Task<LeaveBalanceViewModel> GetLeaveBalanceAsync(int employeeId, int year)
        {
            _logger.LogInformation("Fetching leave balance for employee ID: {EmployeeId}, Year: {Year}", employeeId, year);

            var employee = await _context.Employees.FindAsync(employeeId);
            var leaveBalance = await _context.LeaveBalances
                .FirstOrDefaultAsync(lb => lb.EmployeeId == employeeId && lb.Year == year);

            if (leaveBalance == null)
            {
                leaveBalance = await InitializeLeaveBalanceAsync(employeeId, year);
            }

            return new LeaveBalanceViewModel
            {
                EmployeeId = employeeId,
                EmployeeName = employee?.FullName ?? "",
                Year = year,
                HomeLeaveAccrued = leaveBalance.Home_Leave_Accrued,
                HomeLeaveUsed = leaveBalance.Home_Leave_Used,
                HomeLeaveBalance = leaveBalance.Home_Leave_Accrued - leaveBalance.Home_Leave_Used,
                SickLeaveTotal = leaveBalance.Sick_Leave_Total,
                SickLeaveTaken = leaveBalance.Sick_Leave_Taken,
                SickLeaveBalance = leaveBalance.Sick_Leave_Total - leaveBalance.Sick_Leave_Taken,
                MaternityLeaveTotal = leaveBalance.Maternity_Leave_Total,
                MaternityLeaveUsed = leaveBalance.Maternity_Leave_Used,
                PaternityLeaveTotal = leaveBalance.Paternity_Leave_Total,
                PaternityLeaveUsed = leaveBalance.Paternity_Leave_Used,
                MourningLeaveTotal = leaveBalance.Mourning_Leave_Total,
                MourningLeaveUsed = leaveBalance.Mourning_Leave_Used,
                PublicHolidaysTotal = leaveBalance.Public_Holidays_Total,
                PublicHolidaysUsed = leaveBalance.Public_Holidays_Used
            };
        }

        public async Task<LeaveBalance> InitializeLeaveBalanceAsync(int employeeId, int year)
        {
            _logger.LogInformation("Initializing leave balance for employee ID: {EmployeeId}, Year: {Year}", employeeId, year);

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null) throw new KeyNotFoundException("Employee not found");

            var existing = await _context.LeaveBalances
                .AnyAsync(lb => lb.EmployeeId == employeeId && lb.Year == year);

            if (existing)
                return (await _context.LeaveBalances
                    .FirstOrDefaultAsync(lb => lb.EmployeeId == employeeId && lb.Year == year))!;

            var leaveBalance = new LeaveBalance
            {
                EmployeeId = employeeId,
                Year = year,
                Home_Leave_Accrued = 0,
                Home_Leave_Used = 0,
                Sick_Leave_Total = 12,
                Sick_Leave_Taken = 0,
                Maternity_Leave_Total = employee.Gender == Gender.Female ? 98 : 0,
                Maternity_Leave_Used = 0,
                Maternity_Status = employee.Gender == Gender.Female ? "Eligible" : "NotApplicable",
                Paternity_Leave_Total = employee.Gender == Gender.Male ? 15 : 0,
                Paternity_Leave_Used = 0,
                Mourning_Leave_Total = 13,
                Mourning_Leave_Used = 0,
                Public_Holidays_Total = employee.Gender == Gender.Female ? 14 : 13,
                Public_Holidays_Used = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.LeaveBalances.Add(leaveBalance);
            await _context.SaveChangesAsync();
            return leaveBalance;
        }

        public async Task<decimal> CalculateLeaveEncashmentAsync(int employeeId, int leaveType, decimal days)
        {
            _logger.LogInformation("Calculating leave encashment for employee ID: {EmployeeId}, Leave Type: {LeaveType}, Days: {Days}", employeeId, leaveType, days);

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null) throw new KeyNotFoundException("Employee not found");

            var dailyRate = (employee.BaseSalary + employee.GradeAmount) / 30; // 30 days per month
            var encashmentAmount = dailyRate * days;

            return encashmentAmount;
        }

        public async Task<LeaveEncashmentResult> ProcessLeaveEncashmentAsync(LeaveEncashmentViewModel model)
        {
            _logger.LogInformation("Processing leave encashment for employee ID: {EmployeeId}", model.EmployeeId);

            var leaveBalance = await _context.LeaveBalances
                .FirstOrDefaultAsync(lb => lb.EmployeeId == model.EmployeeId && lb.Year == DateTime.UtcNow.Year);

            if (leaveBalance == null)
            {
                return new LeaveEncashmentResult
                {
                    Success = false,
                    Message = "Leave balance not found"
                };
            }

            var leaveType = (LeaveTypeEnum)model.LeaveType;
            decimal availableDays = 0;
            decimal maxEncashableDays = 0;

            switch (leaveType)
            {
                case LeaveTypeEnum.Home:
                    availableDays = leaveBalance.Home_Leave_Accrued - leaveBalance.Home_Leave_Used - leaveBalance.HomeLeaveEncashed;
                    maxEncashableDays = 90; // Home leave up to 90 days
                    break;
                case LeaveTypeEnum.Sick:
                    availableDays = leaveBalance.Sick_Leave_Total - leaveBalance.Sick_Leave_Taken - leaveBalance.SickLeaveEncashed;
                    maxEncashableDays = 45; // Sick leave up to 45 days upon resignation/retirement
                    break;
                default:
                    return new LeaveEncashmentResult
                    {
                        Success = false,
                        Message = "This leave type is not eligible for encashment"
                    };
            }

            if (model.DaysToEncash > availableDays)
            {
                return new LeaveEncashmentResult
                {
                    Success = false,
                    Message = $"Requested days ({model.DaysToEncash}) exceed available balance ({availableDays})"
                };
            }

            if (model.DaysToEncash > maxEncashableDays)
            {
                return new LeaveEncashmentResult
                {
                    Success = false,
                    Message = $"Maximum encashable days for {leaveType} leave is {maxEncashableDays}"
                };
            }

            var encashmentAmount = await CalculateLeaveEncashmentAsync(model.EmployeeId, model.LeaveType, model.DaysToEncash);

            // Update leave balance
            if (leaveType == LeaveTypeEnum.Home)
                leaveBalance.HomeLeaveEncashed += model.DaysToEncash;
            else
                leaveBalance.SickLeaveEncashed += model.DaysToEncash;

            leaveBalance.UpdatedAt = DateTime.UtcNow;

            // Create a leave request for encashment
            var leaveRequest = new LeaveRequest
            {
                EmployeeId = model.EmployeeId,
                LeaveType = leaveType,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                NumberOfDays = model.DaysToEncash,
                Reason = $"Leave encashment - {model.DaysToEncash} days",
                Status = LeaveStatus.Approved,
                IsEncashment = true,
                EncashmentAmount = encashmentAmount,
                ApprovedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.LeaveRequests.Add(leaveRequest);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Leave encashment processed. Amount: {Amount}, Days: {Days}", encashmentAmount, model.DaysToEncash);

            return new LeaveEncashmentResult
            {
                Success = true,
                Message = "Leave encashment processed successfully",
                EncashmentAmount = encashmentAmount,
                DaysEncashed = model.DaysToEncash,
                LeaveType = model.LeaveType
            };
        }

        public async Task<bool> AccrueHomeLeaveAsync(int employeeId, int workingDays)
        {
            _logger.LogInformation("Accruing home leave for employee ID: {EmployeeId}, Working Days: {WorkingDays}", employeeId, workingDays);

            var leaveBalance = await _context.LeaveBalances
                .FirstOrDefaultAsync(lb => lb.EmployeeId == employeeId && lb.Year == DateTime.UtcNow.Year);

            if (leaveBalance == null) return false;

            // 1 day per 20 working days
            var accruedDays = Math.Floor(workingDays / 20m);
            if (accruedDays <= 0) return false;

            leaveBalance.Home_Leave_Accrued += accruedDays;
            leaveBalance.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Home leave accrued: {AccruedDays} days for employee ID: {EmployeeId}", accruedDays, employeeId);
            return true;
        }

        public async Task<IEnumerable<PublicHoliday>> GetPublicHolidaysAsync(int year)
        {
            return await _context.PublicHolidays
                .Where(ph => ph.HolidayDate.Year == year && ph.IsActive)
                .OrderBy(ph => ph.HolidayDate)
                .ToListAsync();
        }

        public async Task<PublicHoliday> AddPublicHolidayAsync(PublicHoliday holiday)
        {
            _logger.LogInformation("Adding public holiday: {HolidayName}", holiday.HolidayName);
            holiday.CreatedAt = DateTime.UtcNow;
            _context.PublicHolidays.Add(holiday);
            await _context.SaveChangesAsync();
            return holiday;
        }

        public async Task<PublicHoliday> UpdatePublicHolidayAsync(int id, PublicHoliday holiday)
        {
            _logger.LogInformation("Updating public holiday with ID: {Id}", id);
            var existingHoliday = await _context.PublicHolidays.FindAsync(id);
            if (existingHoliday == null)
            {
                throw new KeyNotFoundException($"Public holiday with ID {id} not found");
            }

            // Update properties
            existingHoliday.HolidayName = holiday.HolidayName;
            existingHoliday.HolidayDate = holiday.HolidayDate;
            existingHoliday.NepaliDate = holiday.NepaliDate;
            existingHoliday.IsForWomenOnly = holiday.IsForWomenOnly;
            existingHoliday.Description = holiday.Description;
            existingHoliday.IsActive = holiday.IsActive;

            await _context.SaveChangesAsync();
            return existingHoliday;
        }

        public async Task<bool> DeletePublicHolidayAsync(int id)
        {
            _logger.LogInformation("Deleting public holiday with ID: {Id}", id);
            var holiday = await _context.PublicHolidays.FindAsync(id);
            if (holiday == null)
            {
                return false;
            }

            // Soft delete by setting IsActive to false
            holiday.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetPendingLeaveCountAsync()
        {
            return await _context.LeaveRequests
                .CountAsync(lr => lr.Status == LeaveStatus.Pending);
        }

        public async Task<Dictionary<string, int>> GetLeaveStatsByStatusAsync()
        {
            var stats = await _context.LeaveRequests
                .GroupBy(lr => lr.Status)
                .Select(g => new
                {
                    Status = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToListAsync();
            
            var result = new Dictionary<string, int>();
            foreach (var stat in stats)
            {
                result[stat.Status] = stat.Count;
            }
            
            // Ensure all statuses are present
            if (!result.ContainsKey("Approved")) result["Approved"] = 0;
            if (!result.ContainsKey("Pending")) result["Pending"] = 0;
            if (!result.ContainsKey("Rejected")) result["Rejected"] = 0;
            
            return result;
        }

        private async Task ValidateLeaveBalance(LeaveBalanceViewModel balance, LeaveTypeEnum leaveType, decimal requestedDays, Employee employee)
        {
            switch (leaveType)
            {
                case LeaveTypeEnum.Home:
                    if (requestedDays > balance.HomeLeaveBalance)
                        throw new InvalidOperationException($"Insufficient home leave balance. Available: {balance.HomeLeaveBalance}, Requested: {requestedDays}");
                    break;
                case LeaveTypeEnum.Sick:
                    if (requestedDays > balance.SickLeaveBalance)
                        throw new InvalidOperationException($"Insufficient sick leave balance. Available: {balance.SickLeaveBalance}, Requested: {requestedDays}");
                    break;
                case LeaveTypeEnum.Maternity:
                    if (employee.Gender != Gender.Female)
                        throw new InvalidOperationException("Maternity leave is only applicable for female employees");
                    if (requestedDays > 98)
                        throw new InvalidOperationException("Maternity leave cannot exceed 98 days");
                    break;
                case LeaveTypeEnum.Paternity:
                    if (employee.Gender != Gender.Male)
                        throw new InvalidOperationException("Paternity leave is only applicable for male employees");
                    if (requestedDays > 15)
                        throw new InvalidOperationException("Paternity leave cannot exceed 15 days");
                    break;
                case LeaveTypeEnum.Mourning:
                    if (requestedDays > 13)
                        throw new InvalidOperationException("Mourning leave cannot exceed 13 days");
                    break;
            }
        }

        private void UpdateLeaveBalanceUsed(LeaveBalance balance, LeaveTypeEnum leaveType, decimal days)
        {
            switch (leaveType)
            {
                case LeaveTypeEnum.Home:
                    balance.Home_Leave_Used += days;
                    break;
                case LeaveTypeEnum.Sick:
                    balance.Sick_Leave_Taken += days;
                    break;
                case LeaveTypeEnum.Maternity:
                    balance.Maternity_Leave_Used += days;
                    balance.Maternity_Status = "InProgress";
                    break;
                case LeaveTypeEnum.Paternity:
                    balance.Paternity_Leave_Used += days;
                    break;
                case LeaveTypeEnum.Mourning:
                    balance.Mourning_Leave_Used += days;
                    break;
            }
            balance.UpdatedAt = DateTime.UtcNow;
        }

        public async Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByEmployeeIdAsync(int employeeId)
        {
            return await GetEmployeeLeaveRequestsAsync(employeeId);
        }

        public async Task<LeaveRequest?> GetLeaveRequestByIdAsync(int id)
        {
            return await _context.LeaveRequests.FindAsync(id);
        }

        public async Task<bool> CancelLeaveRequestAsync(int id)
        {
            var leaveRequest = await _context.LeaveRequests.FindAsync(id);
            if (leaveRequest == null) return false;

            if (leaveRequest.Status != LeaveStatus.Pending)
            {
                _logger.LogWarning("Cannot cancel leave request {RequestId} as it is not in pending status", id);
                return false;
            }

            leaveRequest.Status = LeaveStatus.Cancelled;
            leaveRequest.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Leave request {RequestId} cancelled successfully", id);
            return true;
        }

        public async Task<LeaveRequest> CreateLeaveRequestAsync(LeaveRequest leaveRequest)
        {
            leaveRequest.CreatedAt = DateTime.UtcNow;
            _context.LeaveRequests.Add(leaveRequest);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Leave request created successfully. Request ID: {RequestId}", leaveRequest.Id);
            return leaveRequest;
        }
    }
}
