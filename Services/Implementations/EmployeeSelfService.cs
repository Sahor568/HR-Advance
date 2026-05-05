using Microsoft.EntityFrameworkCore;
using HR_Management_System.Data;
using HR_Management_System.Models;
using HR_Management_System.Models.Performance;
using HR_Management_System.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HR_Management_System.Services.Implementations
{
    public class EmployeeSelfService : IEmployeeSelfService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EmployeeSelfService> _logger;

        public EmployeeSelfService(ApplicationDbContext context, ILogger<EmployeeSelfService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Employee Dashboard
        public async Task<object> GetEmployeeDashboardAsync(int employeeId)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(employeeId);
                if (employee == null)
                    throw new ArgumentException("Employee not found");

                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                var dashboardData = new
                {
                    Employee = new
                    {
                        employee.Id,
                        employee.FullName,
                        employee.Designation,
                        employee.Department,
                        employee.Join_Date
                    },
                    LeaveBalance = await GetMyLeaveBalanceAsync(employeeId),
                    AttendanceSummary = await GetMyAttendanceSummaryAsync(employeeId, currentMonth, currentYear),
                    UpcomingEvents = await GetUpcomingEventsAsync(employeeId),
                    PendingApprovals = await GetPendingMyApprovalsAsync(employeeId),
                    RecentActivities = await GetRecentActivitiesAsync(employeeId),
                    UnreadNotifications = await GetUnreadNotificationCountAsync(employeeId)
                };

                return dashboardData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting dashboard data for employee {employeeId}");
                throw;
            }
        }

        public async Task<object> GetUpcomingEventsAsync(int employeeId)
        {
            try
            {
                var events = new List<object>();
                
                // Get upcoming birthdays
                var upcomingBirthdays = await _context.Employees
                    .Where(e => e.DateOfBirth.Month == DateTime.Now.Month && e.DateOfBirth.Day >= DateTime.Now.Day)
                    .Take(5)
                    .Select(e => new { Type = "Birthday", Name = e.FullName, Date = e.DateOfBirth })
                    .ToListAsync();

                events.AddRange(upcomingBirthdays);

                // Get upcoming holidays
                var upcomingHolidays = await _context.PublicHolidays
                    .Where(h => h.Date >= DateTime.Now)
                    .OrderBy(h => h.Date)
                    .Take(5)
                    .Select(h => new { Type = "Holiday", Name = h.Name, Date = h.Date })
                    .ToListAsync();

                events.AddRange(upcomingHolidays);

                // Get upcoming trainings
                var upcomingTrainings = await _context.Trainings
                    .Where(t => t.StartDate >= DateTime.Now && t.Participants.Any(p => p.EmployeeId == employeeId))
                    .OrderBy(t => t.StartDate)
                    .Take(5)
                    .Select(t => new { Type = "Training", Name = t.Title, Date = t.StartDate })
                    .ToListAsync();

                events.AddRange(upcomingTrainings);

                return events;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting upcoming events for employee {employeeId}");
                throw;
            }
        }

        public async Task<object> GetPendingApprovalsAsync(int employeeId)
        {
            try
            {
                var pendingApprovals = new List<object>();

                // Get pending leave requests to approve
                var pendingLeaveRequests = await _context.LeaveRequests
                    .Where(l => l.ApproverId == employeeId && l.Status == LeaveStatus.Pending)
                    .Select(l => new
                    {
                        Type = "Leave Request",
                        Id = l.Id,
                        EmployeeName = l.Employee.FullName,
                        StartDate = l.StartDate,
                        EndDate = l.EndDate,
                        LeaveType = l.LeaveType,
                        AppliedDate = l.AppliedDate
                    })
                    .ToListAsync();

                pendingApprovals.AddRange(pendingLeaveRequests);

                // Get pending travel requests to approve
                var pendingTravelRequests = await _context.TravelRequests
                    .Where(t => t.ApproverId == employeeId && t.Status == "Pending")
                    .Select(t => new
                    {
                        Type = "Travel Request",
                        Id = t.Id,
                        EmployeeName = t.Employee.FullName,
                        Purpose = t.Purpose,
                        StartDate = t.StartDate,
                        EndDate = t.EndDate,
                        AppliedDate = t.CreatedDate
                    })
                    .ToListAsync();

                pendingApprovals.AddRange(pendingTravelRequests);

                // Get pending reimbursement claims to approve
                var pendingReimbursements = await _context.Reimbursements
                    .Where(r => r.ApproverId == employeeId && r.Status == "Pending")
                    .Select(r => new
                    {
                        Type = "Reimbursement",
                        Id = r.Id,
                        EmployeeName = r.Employee.FullName,
                        Amount = r.TotalAmount,
                        Purpose = r.Purpose,
                        AppliedDate = r.CreatedDate
                    })
                    .ToListAsync();

                pendingApprovals.AddRange(pendingReimbursements);

                return pendingApprovals;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting pending approvals for employee {employeeId}");
                throw;
            }
        }

        public async Task<object> GetRecentActivitiesAsync(int employeeId)
        {
            try
            {
                var activities = new List<object>();

                // Get recent leave requests
                var recentLeaves = await _context.LeaveRequests
                    .Where(l => l.EmployeeId == employeeId)
                    .OrderByDescending(l => l.AppliedDate)
                    .Take(5)
                    .Select(l => new
                    {
                        Type = "Leave",
                        Action = l.Status == LeaveStatus.Approved ? "Approved" : 
                                 l.Status == LeaveStatus.Rejected ? "Rejected" : "Applied",
                        Date = l.AppliedDate,
                        Details = $"{l.LeaveType} leave from {l.StartDate:dd-MMM} to {l.EndDate:dd-MMM}"
                    })
                    .ToListAsync();

                activities.AddRange(recentLeaves);

                // Get recent attendance
                var recentAttendance = await _context.Attendances
                    .Where(a => a.EmployeeId == employeeId)
                    .OrderByDescending(a => a.Date)
                    .Take(5)
                    .Select(a => new
                    {
                        Type = "Attendance",
                        Action = "Recorded",
                        Date = a.Date,
                        Details = $"Clock In: {a.Clock_In:HH:mm}, Clock Out: {a.Clock_Out:HH:mm}"
                    })
                    .ToListAsync();

                activities.AddRange(recentAttendance);

                // Get recent payroll
                var recentPayroll = await _context.Payrolls
                    .Where(p => p.EmployeeId == employeeId)
                    .OrderByDescending(p => p.Month)
                    .Take(5)
                    .Select(p => new
                    {
                        Type = "Payroll",
                        Action = "Processed",
                        Date = new DateTime(p.Year, p.Month, 1),
                        Details = $"Salary: NPR {p.NetPay:N2} for {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(p.Month)} {p.Year}"
                    })
                    .ToListAsync();

                activities.AddRange(recentPayroll);

                return activities.OrderByDescending(a => ((dynamic)a).Date).Take(10);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting recent activities for employee {employeeId}");
                throw;
            }
        }

        // Personal Information Management
        public async Task<Employee> GetEmployeeProfileAsync(int employeeId)
        {
            try
            {
                return await _context.Employees
                    .Include(e => e.Attendances)
                    .Include(e => e.LeaveBalances)
                    .Include(e => e.LeaveRequests)
                    .FirstOrDefaultAsync(e => e.Id == employeeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting profile for employee {employeeId}");
                throw;
            }
        }

        public async Task<bool> UpdatePersonalInformationAsync(int employeeId, Dictionary<string, object> updates)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(employeeId);
                if (employee == null)
                    return false;

                foreach (var update in updates)
                {
                    var property = typeof(Employee).GetProperty(update.Key);
                    if (property != null && property.CanWrite)
                    {
                        property.SetValue(employee, Convert.ChangeType(update.Value, property.PropertyType));
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating personal information for employee {employeeId}");
                throw;
            }
        }

        public async Task<bool> UpdateContactInformationAsync(int employeeId, string phone, string email, string address)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(employeeId);
                if (employee == null)
                    return false;

                employee.PhoneNumber = phone;
                employee.Email = email;
                employee.TemporaryAddress = address;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating contact information for employee {employeeId}");
                throw;
            }
        }

        public async Task<bool> UploadDocumentAsync(int employeeId, string documentType, string filePath, string fileName)
        {
            try
            {
                // Implementation for document upload
                // This would typically save to a document storage system
                // For now, we'll just log it
                _logger.LogInformation($"Employee {employeeId} uploaded document: {fileName} of type {documentType}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading document for employee {employeeId}");
                throw;
            }
        }

        public async Task<IEnumerable<object>> GetEmployeeDocumentsAsync(int employeeId)
        {
            try
            {
                // Implementation for getting employee documents
                // This would typically retrieve from a document storage system
                // For now, return empty list
                return new List<object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting documents for employee {employeeId}");
                throw;
            }
        }

        // Leave Management (Self Service)
        public async Task<LeaveRequest> ApplyForLeaveAsync(int employeeId, LeaveRequest leaveRequest)
        {
            try
            {
                leaveRequest.EmployeeId = employeeId;
                leaveRequest.Status = LeaveStatus.Pending;
                leaveRequest.AppliedDate = DateTime.Now;

                _context.LeaveRequests.Add(leaveRequest);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Employee {employeeId} applied for leave from {leaveRequest.StartDate} to {leaveRequest.EndDate}");
                return leaveRequest;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error applying for leave for employee {employeeId}");
                throw;
            }
        }

        public async Task<IEnumerable<LeaveRequest>> GetMyLeaveRequestsAsync(int employeeId)
        {
            try
            {
                return await _context.LeaveRequests
                    .Where(l => l.EmployeeId == employeeId)
                    .OrderByDescending(l => l.AppliedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting leave requests for employee {employeeId}");
                throw;
            }
        }

        public async Task<LeaveRequest> GetLeaveRequestDetailsAsync(int employeeId, int leaveRequestId)
        {
            try
            {
                return await _context.LeaveRequests
                    .FirstOrDefaultAsync(l => l.Id == leaveRequestId && l.EmployeeId == employeeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting leave request details {leaveRequestId} for employee {employeeId}");
                throw;
            }
        }

        public async Task<bool> CancelLeaveRequestAsync(int employeeId, int leaveRequestId, string reason)
        {
            try
            {
                var leaveRequest = await _context.LeaveRequests
                    .FirstOrDefaultAsync(l => l.Id == leaveRequestId && l.EmployeeId == employeeId);

                if (leaveRequest == null)
                    return false;

                if (leaveRequest.Status != LeaveStatus.Pending)
                    return false;

                leaveRequest.Status = LeaveStatus.Cancelled;
                leaveRequest.CancellationReason = reason;
                leaveRequest.CancelledDate = DateTime.Now;

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Employee {employeeId} cancelled leave request {leaveRequestId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error cancelling leave request {leaveRequestId} for employee {employeeId}");
                throw;
            }
        }

        public async Task<object> GetMyLeaveBalanceAsync(int employeeId)
        {
            try
            {
                var currentYear = DateTime.Now.Year;
                var leaveBalance = await _context.LeaveBalances
                    .FirstOrDefaultAsync(lb => lb.EmployeeId == employeeId && lb.Year == currentYear);

                if (leaveBalance == null)
                {
                    // Create default leave balance if not exists
                    leaveBalance = new LeaveBalance
                    {
                        EmployeeId = employeeId,
                        Year = currentYear,
                        HomeLeaveAccrued = 0,
                        HomeLeaveTaken = 0,
                        SickLeaveTaken = 0,
                        MaternityLeaveTaken = 0,
                        PaternityLeaveTaken = 0,
                        MourningLeaveTaken = 0
                    };
                    _context.LeaveBalances.Add(leaveBalance);
                    await _context.SaveChangesAsync();
                }

                return leaveBalance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting leave balance for employee {employeeId}");
                throw;
            }
        }

        // Attendance & Timesheet (Self Service)
        public async Task<bool> ClockInAsync(int employeeId, string location = null, string deviceId = null)
        {
            try
            {
                var today = DateTime.Today;
                var existingAttendance = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == today);

                if (existingAttendance != null)
                {
                    // Already clocked in today
                    return false;
                }

                var attendance = new Attendance
                {
                    EmployeeId = employeeId,
                    Date = today,
                    Clock_In = DateTime.Now.TimeOfDay,
                    Status = AttendanceStatus.Present,
                    Location = location,
                    DeviceId = deviceId
                };

                _context.Attendances.Add(attendance);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Employee {employeeId} clocked in at {attendance.Clock_In}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error clocking in for employee {employeeId}");
                throw;
            }
        }

        public async Task<bool> ClockOutAsync(int employeeId, string location = null, string deviceId = null)
        {
            try
            {
                var today = DateTime.Today;
                var attendance = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == today);

                if (attendance == null)
                {
                    // Not clocked in today
                    return false;
                }

                if (attendance.Clock_Out.HasValue)
                {
                    // Already clocked out today
                    return false;
                }

                attendance.Clock_Out = DateTime.Now.TimeOfDay;
                attendance.Location = location ?? attendance.Location;
                attendance.DeviceId = deviceId ?? attendance.DeviceId;

                // Calculate working hours
                var clockInTime = DateTime.Today.Add(attendance.Clock_In);
                var clockOutTime = DateTime.Now;
                var workingHours = (clockOutTime - clockInTime).TotalHours;

                // Calculate overtime (more than 8 hours)
                if (workingHours > 8)
                {
                    attendance.OT_Hours = (decimal)(workingHours - 8);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Employee {employeeId} clocked out at {attendance.Clock_Out}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error clocking out for employee {employeeId}");
                throw;
            }
        }

        public async Task<object> GetMyAttendanceSummaryAsync(int employeeId, int month, int year)
        {
            try
            {
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var attendances = await _context.Attendances
                    .Where(a => a.EmployeeId == employeeId && a.Date >= startDate && a.Date <= endDate)
                    .ToListAsync();

                var summary = new
                {
                    TotalDays = attendances.Count,
                    PresentDays = attendances.Count(a => a.Status == AttendanceStatus.Present),
                    AbsentDays = attendances.Count(a => a.Status == AttendanceStatus.Absent),
                    HalfDays = attendances.Count(a => a.Status == AttendanceStatus.HalfDay),
                    TotalOTHours = attendances.Sum(a => a.OT_Hours ?? 0),
                    AverageWorkingHours = attendances.Where(a => a.Clock_Out.HasValue && a.Clock_In.HasValue)
                        .Average(a => (a.Clock_Out.Value - a.Clock_In.Value).TotalHours)
                };

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting attendance summary for employee {employeeId}");
                throw;
            }
        }

        public async Task<IEnumerable<Attendance>> GetMyAttendanceRecordsAsync(int employeeId, DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _context.Attendances
                    .Where(a => a.EmployeeId == employeeId && a.Date >= startDate && a.Date <= endDate)
                    .OrderByDescending(a => a.Date)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting attendance records for employee {