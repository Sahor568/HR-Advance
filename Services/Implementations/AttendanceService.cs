using Microsoft.EntityFrameworkCore;
using HR_Management_System.Data;
using HR_Management_System.Models;
using HR_Management_System.Models.Enums;
using HR_Management_System.Models.ViewModels;
using HR_Management_System.Services.Interfaces;

namespace HR_Management_System.Services.Implementations
{
    public class AttendanceService : IAttendanceService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AttendanceService> _logger;

        public AttendanceService(ApplicationDbContext context, ILogger<AttendanceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Attendance> ClockInAsync(int employeeId, decimal? latitude = null, decimal? longitude = null, string? locationAddress = null)
        {
            _logger.LogInformation("Clock-in for employee ID: {EmployeeId}", employeeId);

            // Get Nepal time (UTC+5:45)
            var nepalTime = GetNepalTime();
            var today = nepalTime.Date;
            var clockInTime = nepalTime.TimeOfDay;

            // Check if today is weekend (Saturday or Sunday)
            if (IsWeekend(today))
            {
                _logger.LogWarning("Employee {EmployeeId} attempted to clock in on weekend: {DayOfWeek}", employeeId, today.DayOfWeek);
                throw new InvalidOperationException("Attendance cannot be marked on weekends (Saturday/Sunday)");
            }

            // Check if today is a public holiday
            try
            {
                if (await IsPublicHolidayAsync(today))
                {
                    _logger.LogWarning("Employee {EmployeeId} attempted to clock in on public holiday: {Date}", employeeId, today);
                    throw new InvalidOperationException("Attendance cannot be marked on public holidays");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking public holiday for date {Date}. Continuing with clock-in.", today);
                // Continue with clock-in if we can't check holiday status
            }

            // Check if employee exists
            var employeeExists = await _context.Employees.AnyAsync(e => e.Id == employeeId);
            if (!employeeExists)
            {
                _logger.LogWarning("Employee {EmployeeId} not found", employeeId);
                throw new InvalidOperationException($"Employee with ID {employeeId} not found");
            }

            var existingAttendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == today);

            if (existingAttendance != null)
            {
                _logger.LogWarning("Employee {EmployeeId} already clocked in today", employeeId);
                throw new InvalidOperationException("Already clocked in today");
            }

            // Check if location is enabled (required for attendance)
            bool isLocationEnabled = latitude.HasValue && longitude.HasValue || !string.IsNullOrEmpty(locationAddress);
            if (!isLocationEnabled)
            {
                _logger.LogWarning("Employee {EmployeeId} attempted to clock in without location enabled", employeeId);
                throw new InvalidOperationException("Location must be enabled to mark attendance");
            }

            // Determine status based on clock-in time
            // Office starts at 9:00 AM, 30 minutes grace period until 9:30 AM
            var officeStartTime = new TimeSpan(9, 0, 0); // 9:00 AM
            var lateThreshold = new TimeSpan(9, 30, 0); // 9:30 AM
            
            AttendanceStatus status;
            if (clockInTime <= lateThreshold)
            {
                status = AttendanceStatus.Present;
            }
            else
            {
                // More than 30 minutes late, requires HR/admin approval
                status = AttendanceStatus.Pending;
                _logger.LogInformation("Employee {EmployeeId} clocked in late at {ClockInTime}. Status set to Pending for approval.", employeeId, clockInTime);
            }

            var attendance = new Attendance
            {
                EmployeeId = employeeId,
                Date = today,
                Clock_In = clockInTime,
                Status = status,
                OT_Hours = 0,
                TotalHours = 0,
                CreatedAt = DateTime.UtcNow,
                Latitude = latitude,
                Longitude = longitude,
                LocationAddress = locationAddress,
                IsLocationEnabled = isLocationEnabled,
                NepaliDate = GetNepalDate(today),
                Remarks = status == AttendanceStatus.Pending ? $"Late arrival at {clockInTime}. Requires HR/Admin approval." : null
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Clock-in successful for employee ID: {EmployeeId} with status: {Status}", employeeId, status);
            return attendance;
        }

        public async Task<Attendance> ClockOutAsync(int employeeId)
        {
            _logger.LogInformation("Clock-out for employee ID: {EmployeeId}", employeeId);

            // Get Nepal time for clock-out
            var nepalTime = GetNepalTime();
            var today = nepalTime.Date;
            
            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == today);

            if (attendance == null)
            {
                _logger.LogWarning("No clock-in record found for employee {EmployeeId} today", employeeId);
                throw new InvalidOperationException("No clock-in record found for today");
            }

            if (attendance.Clock_Out.HasValue)
            {
                _logger.LogWarning("Employee {EmployeeId} already clocked out today", employeeId);
                throw new InvalidOperationException("Already clocked out today");
            }

            // Use Nepal time for clock-out
            attendance.Clock_Out = nepalTime.TimeOfDay;
            attendance.TotalHours = (decimal)(attendance.Clock_Out.Value - attendance.Clock_In).TotalHours;

            // Calculate overtime: standard 8 hours/day (office hours 9:00 AM to 5:30 PM = 8.5 hours)
            // But overtime calculation starts after 8 hours of work
            if (attendance.TotalHours > 8)
            {
                attendance.OT_Hours = attendance.TotalHours - 8;
                _logger.LogInformation("Overtime detected for employee {EmployeeId}: {OTHours} hours", employeeId, attendance.OT_Hours);
            }

            attendance.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Clock-out successful for employee ID: {EmployeeId}, Total Hours: {TotalHours}, Status: {Status}",
                employeeId, attendance.TotalHours, attendance.Status);
            return attendance;
        }

        public async Task<IEnumerable<AttendanceListViewModel>> GetAttendanceByDateRangeAsync(AttendanceFilterViewModel filter)
        {
            _logger.LogInformation("Fetching attendance records with filter");

            var query = _context.Attendances
                .Include(a => a.Employee)
                .AsQueryable();

            if (filter.EmployeeId.HasValue)
                query = query.Where(a => a.EmployeeId == filter.EmployeeId.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(a => a.Date >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(a => a.Date <= filter.ToDate.Value);

            if (filter.Status.HasValue)
                query = query.Where(a => a.Status == (AttendanceStatus)filter.Status.Value);

            if (!string.IsNullOrEmpty(filter.Department))
                query = query.Where(a => a.Employee.Department == filter.Department);

            if (!filter.IncludeWeekends)
                query = query.Where(a => !a.IsWeekend);

            if (!filter.IncludeHolidays)
                query = query.Where(a => !a.IsHoliday);

            return await query
                .OrderByDescending(a => a.Date)
                .ThenBy(a => a.EmployeeId)
                .Select(a => new AttendanceListViewModel
                {
                    Id = a.Id,
                    Emp_ID = a.Employee.Emp_ID,
                    EmployeeName = a.Employee.FullName,
                    Date = a.Date,
                    NepaliDate = a.NepaliDate,
                    Clock_In = a.Clock_In,
                    Clock_Out = a.Clock_Out,
                    TotalHours = a.TotalHours,
                    OT_Hours = a.OT_Hours,
                    Status = a.Status.ToString(),
                    Remarks = a.Remarks,
                    IsHoliday = a.IsHoliday,
                    IsWeekend = a.IsWeekend,
                    HolidayName = a.HolidayName
                })
                .Take(500)
                .ToListAsync();
        }

        public async Task<Attendance?> GetTodayAttendanceAsync(int employeeId)
        {
            // Use Nepal time for today's date
            var nepalTime = GetNepalTime();
            var today = nepalTime.Date;
            return await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == today);
        }

        public async Task<IEnumerable<AttendanceListViewModel>> GetTodayAttendancesAsync()
        {
            // Use Nepal time for today's date
            var nepalTime = GetNepalTime();
            var today = nepalTime.Date;
            
            return await _context.Attendances
                .Where(a => a.Date == today)
                .Include(a => a.Employee)
                .Select(a => new AttendanceListViewModel
                {
                    Id = a.Id,
                    Emp_ID = a.EmployeeId.ToString(),
                    EmployeeName = a.Employee.FullName,
                    Date = a.Date,
                    NepaliDate = a.NepaliDate,
                    Clock_In = a.Clock_In,
                    Clock_Out = a.Clock_Out,
                    Status = a.Status.ToString(),
                    TotalHours = a.TotalHours,
                    OT_Hours = a.OT_Hours,
                    Remarks = a.Remarks,
                    IsHoliday = a.IsHoliday,
                    IsWeekend = a.IsWeekend,
                    HolidayName = a.HolidayName
                })
                .ToListAsync();
        }

        public async Task<Dictionary<DateTime, (int Present, int Absent)>> GetAttendanceStatsLast7DaysAsync()
        {
            var endDate = DateTime.UtcNow.Date;
            var startDate = endDate.AddDays(-6);
            
            var activeEmployeeIds = await _context.Employees
                .Where(e => e.IsActive)
                .Select(e => e.Id)
                .ToListAsync();
            
            var attendanceByDate = await _context.Attendances
                .Where(a => a.Date >= startDate && a.Date <= endDate)
                .GroupBy(a => a.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    PresentCount = g.Count(a => a.Status == Models.Enums.AttendanceStatus.Present),
                    AbsentCount = g.Count(a => a.Status == Models.Enums.AttendanceStatus.Absent)
                })
                .ToDictionaryAsync(
                    x => x.Date,
                    x => (x.PresentCount, x.AbsentCount)
                );
            
            var result = new Dictionary<DateTime, (int Present, int Absent)>();
            
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (attendanceByDate.TryGetValue(date, out var counts))
                {
                    result[date] = counts;
                }
                else
                {
                    // If no attendance records for this date, count all active employees as absent
                    result[date] = (0, activeEmployeeIds.Count);
                }
            }
            
            return result;
        }

        public async Task<IEnumerable<WeeklyWorkHoursViewModel>> GetWeeklyWorkHoursAsync(int employeeId, DateTime? weekStart = null)
        {
            _logger.LogInformation("Calculating weekly work hours for employee ID: {EmployeeId}", employeeId);

            var startDate = weekStart ?? DateTime.UtcNow.StartOfWeek(DayOfWeek.Sunday);
            var endDate = startDate.AddDays(6);

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null) return Enumerable.Empty<WeeklyWorkHoursViewModel>();

            var weekAttendances = await _context.Attendances
                .Where(a => a.EmployeeId == employeeId && a.Date >= startDate && a.Date <= endDate)
                .ToListAsync();

            var totalHours = weekAttendances.Sum(a => a.TotalHours);
            var overtimeHours = weekAttendances.Sum(a => a.OT_Hours);

            var result = new WeeklyWorkHoursViewModel
            {
                EmployeeId = employeeId,
                EmployeeName = employee.FullName,
                WeekStart = startDate,
                WeekEnd = endDate,
                TotalHoursWorked = totalHours,
                OvertimeHours = overtimeHours
            };

            if (totalHours > 48)
            {
                _logger.LogWarning("Employee {EmployeeId} exceeds 48-hour weekly limit: {TotalHours} hours", employeeId, totalHours);
            }

            return new List<WeeklyWorkHoursViewModel> { result };
        }

        public async Task<bool> MarkAbsentAsync(int employeeId, DateTime date)
        {
            _logger.LogInformation("Marking absent for employee ID: {EmployeeId} on {Date}", employeeId, date);

            var existing = await _context.Attendances
                .AnyAsync(a => a.EmployeeId == employeeId && a.Date == date);

            if (existing) return false;

            var isWeekend = IsWeekend(date);
            var holiday = await GetHolidayAsync(date);

            var attendance = new Attendance
            {
                EmployeeId = employeeId,
                Date = date,
                Clock_In = TimeSpan.Zero,
                Status = AttendanceStatus.Absent,
                OT_Hours = 0,
                TotalHours = 0,
                CreatedAt = DateTime.UtcNow,
                NepaliDate = GetNepalDate(date),
                IsWeekend = isWeekend,
                IsHoliday = holiday != null,
                HolidayName = holiday?.HolidayName
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> CalculateOvertimeAsync(int employeeId, DateTime date)
        {
            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == date);

            return attendance?.OT_Hours ?? 0;
        }

        public async Task<Dictionary<int, decimal>> GetMonthlyOTSummaryAsync(int month, int year)
        {
            _logger.LogInformation("Fetching monthly OT summary for {Month}/{Year}", month, year);

            return await _context.Attendances
                .Where(a => a.Date.Month == month && a.Date.Year == year && a.OT_Hours > 0)
                .GroupBy(a => a.EmployeeId)
                .ToDictionaryAsync(g => g.Key, g => g.Sum(a => a.OT_Hours));
        }

        public async Task CheckAndMarkAbsencesAsync()
        {
            _logger.LogInformation("Running absence check for today");

            // Use Nepal time for today's date
            var nepalTime = GetNepalTime();
            var today = nepalTime.Date;

            // Skip weekends using helper method
            if (IsWeekend(today))
            {
                _logger.LogInformation("Today is a weekend, skipping absence check");
                return;
            }

            // Check if today is a public holiday
            var isHoliday = await IsPublicHolidayAsync(today);

            if (isHoliday)
            {
                _logger.LogInformation("Today is a public holiday, skipping absence check");
                return;
            }

            var activeEmployees = await _context.Employees
                .Where(e => e.IsActive)
                .Select(e => e.Id)
                .ToListAsync();

            var alreadyMarked = await _context.Attendances
                .Where(a => a.Date == today)
                .Select(a => a.EmployeeId)
                .ToListAsync();

            var absentEmployees = activeEmployees.Except(alreadyMarked).ToList();

            foreach (var empId in absentEmployees)
            {
                await MarkAbsentAsync(empId, today);
            }

            _logger.LogInformation("Marked {Count} employees as absent", absentEmployees.Count);
        }

        // Get pending attendances that require HR/admin approval
        public async Task<IEnumerable<AttendanceListViewModel>> GetPendingAttendancesAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            _logger.LogInformation("Fetching pending attendance records");
            
            var query = _context.Attendances
                .Include(a => a.Employee)
                .Where(a => a.Status == AttendanceStatus.Pending)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(a => a.Date >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.Date <= toDate.Value);

            return await query
                .OrderByDescending(a => a.Date)
                .ThenBy(a => a.EmployeeId)
                .Select(a => new AttendanceListViewModel
                {
                    Id = a.Id,
                    EmployeeId = a.EmployeeId,
                    Emp_ID = a.Employee.Emp_ID,
                    EmployeeName = a.Employee.FullName,
                    Date = a.Date,
                    NepaliDate = a.NepaliDate,
                    Clock_In = a.Clock_In,
                    Clock_Out = a.Clock_Out,
                    TotalHours = a.TotalHours,
                    OT_Hours = a.OT_Hours,
                    Status = a.Status.ToString(),
                    Remarks = a.Remarks,
                    Latitude = a.Latitude,
                    Longitude = a.Longitude,
                    LocationAddress = a.LocationAddress,
                    IsHoliday = a.IsHoliday,
                    IsWeekend = a.IsWeekend,
                    HolidayName = a.HolidayName
                })
                .Take(100)
                .ToListAsync();
        }

        // Get count of pending attendance requests
        public async Task<int> GetPendingAttendanceCountAsync()
        {
            return await _context.Attendances
                .Where(a => a.Status == AttendanceStatus.Pending)
                .CountAsync();
        }

        // Approve a pending attendance
        public async Task<bool> ApproveAttendanceAsync(int attendanceId, string approvedBy, string? remarks = null)
        {
            _logger.LogInformation("Approving attendance ID: {AttendanceId} by {ApprovedBy}", attendanceId, approvedBy);

            var attendance = await _context.Attendances.FindAsync(attendanceId);
            if (attendance == null)
            {
                _logger.LogWarning("Attendance record not found: {AttendanceId}", attendanceId);
                return false;
            }

            if (attendance.Status != AttendanceStatus.Pending)
            {
                _logger.LogWarning("Attendance ID {AttendanceId} is not pending (current status: {Status})", attendanceId, attendance.Status);
                return false;
            }

            attendance.Status = AttendanceStatus.Present;
            attendance.Remarks = $"Approved by {approvedBy} on {DateTime.UtcNow:yyyy-MM-dd HH:mm}" +
                                (string.IsNullOrEmpty(remarks) ? "" : $". Remarks: {remarks}");
            attendance.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Attendance ID {AttendanceId} approved successfully", attendanceId);
            return true;
        }

        // Reject a pending attendance (mark as absent)
        public async Task<bool> RejectAttendanceAsync(int attendanceId, string rejectedBy, string? remarks = null)
        {
            _logger.LogInformation("Rejecting attendance ID: {AttendanceId} by {RejectedBy}", attendanceId, rejectedBy);

            var attendance = await _context.Attendances.FindAsync(attendanceId);
            if (attendance == null)
            {
                _logger.LogWarning("Attendance record not found: {AttendanceId}", attendanceId);
                return false;
            }

            if (attendance.Status != AttendanceStatus.Pending)
            {
                _logger.LogWarning("Attendance ID {AttendanceId} is not pending (current status: {Status})", attendanceId, attendance.Status);
                return false;
            }

            attendance.Status = AttendanceStatus.Absent;
            attendance.Remarks = $"Rejected by {rejectedBy} on {DateTime.UtcNow:yyyy-MM-dd HH:mm}" +
                                (string.IsNullOrEmpty(remarks) ? "" : $". Remarks: {remarks}");
            attendance.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Attendance ID {AttendanceId} rejected and marked as absent", attendanceId);
            return true;
        }

        public async Task<bool> IsTodayHolidayOrWeekendAsync()
        {
            var nepalTime = GetNepalTime();
            var today = nepalTime.Date;
            
            if (IsWeekend(today)) return true;
            
            return await IsPublicHolidayAsync(today);
        }

        // Helper method to get current Nepal time (UTC+5:45)
        private DateTime GetNepalTime()
        {
            var utcNow = DateTime.UtcNow;
            
            try
            {
                // Try to get Nepal timezone - different names on different systems
                TimeZoneInfo nepalTimeZone = null;
                
                // Try common timezone IDs for Nepal
                string[] timezoneIds = new[]
                {
                    "Nepal Standard Time",
                    "Asia/Katmandu",
                    "Asia/Kathmandu",
                    "Nepal Time"
                };
                
                foreach (var tzId in timezoneIds)
                {
                    try
                    {
                        nepalTimeZone = TimeZoneInfo.FindSystemTimeZoneById(tzId);
                        if (nepalTimeZone != null)
                            break;
                    }
                    catch { }
                }
                
                if (nepalTimeZone != null)
                {
                    return TimeZoneInfo.ConvertTimeFromUtc(utcNow, nepalTimeZone);
                }
                
                // If no timezone found, use manual offset UTC+5:45
                return utcNow.AddHours(5).AddMinutes(45);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get Nepal timezone, using manual offset UTC+5:45");
                // Fallback: manually add 5 hours 45 minutes
                return utcNow.AddHours(5).AddMinutes(45);
            }
        }

        // Helper method to check if a date is weekend (Saturday or Sunday)
        private bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }

        // Helper method to check if a date is a public holiday
        private async Task<bool> IsPublicHolidayAsync(DateTime date)
        {
            return await _context.PublicHolidays
                .AnyAsync(ph => ph.HolidayDate.Date == date && ph.IsActive);
        }

        // Helper method to get Nepali date (BS calendar)
        private string GetNepalDate(DateTime englishDate)
        {
            // BS calendar reference: 2000/01/01 BS = 1943/04/14 AD
            var bsYear = englishDate.Year - 56;
            var bsMonth = englishDate.Month;
            var bsDay = englishDate.Day;

            // Rough Nepali date conversion (more accurate conversion would need lookup table)
            // Approximate: Baisakh approx March/April, Jestha May/June, etc.
            if (englishDate.Month <= 3)
                bsMonth = englishDate.Month + 9; // Jan-Mar = 10-12 BS months
            else if (englishDate.Month <= 12)
                bsMonth = englishDate.Month - 3; // Apr-Dec = 1-9 BS months

            // Handle day adjustments
            bsDay = Math.Min(bsDay, 32); // Cap at reasonable day

            return $"{bsYear}/{bsMonth:D2}/{bsDay:D2}";
        }

        // Helper method to check if a date is a holiday
        private async Task<PublicHoliday?> GetHolidayAsync(DateTime date)
        {
            return await _context.PublicHolidays
                .FirstOrDefaultAsync(ph => ph.HolidayDate.Date == date && ph.IsActive);
        }
    }

    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
}
