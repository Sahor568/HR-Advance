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

            var today = DateTime.UtcNow.Date;
            var existingAttendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == today);

            if (existingAttendance != null)
            {
                _logger.LogWarning("Employee {EmployeeId} already clocked in today", employeeId);
                throw new InvalidOperationException("Already clocked in today");
            }

            var attendance = new Attendance
            {
                EmployeeId = employeeId,
                Date = today,
                Clock_In = DateTime.UtcNow.TimeOfDay,
                Status = AttendanceStatus.Present,
                OT_Hours = 0,
                TotalHours = 0,
                CreatedAt = DateTime.UtcNow,
                Latitude = latitude,
                Longitude = longitude,
                LocationAddress = locationAddress,
                IsLocationEnabled = latitude.HasValue && longitude.HasValue || !string.IsNullOrEmpty(locationAddress)
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Clock-in successful for employee ID: {EmployeeId} with location enabled: {LocationEnabled}", employeeId, attendance.IsLocationEnabled);
            return attendance;
        }

        public async Task<Attendance> ClockOutAsync(int employeeId)
        {
            _logger.LogInformation("Clock-out for employee ID: {EmployeeId}", employeeId);

            var today = DateTime.UtcNow.Date;
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

            attendance.Clock_Out = DateTime.UtcNow.TimeOfDay;
            attendance.TotalHours = (decimal)(attendance.Clock_Out.Value - attendance.Clock_In).TotalHours;

            // Calculate overtime: standard 8 hours/day
            if (attendance.TotalHours > 8)
            {
                attendance.OT_Hours = attendance.TotalHours - 8;
                _logger.LogInformation("Overtime detected for employee {EmployeeId}: {OTHours} hours", employeeId, attendance.OT_Hours);
            }

            attendance.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Clock-out successful for employee ID: {EmployeeId}, Total Hours: {TotalHours}", employeeId, attendance.TotalHours);
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

            return await query
                .OrderByDescending(a => a.Date)
                .ThenBy(a => a.EmployeeId)
                .Select(a => new AttendanceListViewModel
                {
                    Id = a.Id,
                    Emp_ID = a.Employee.Emp_ID,
                    EmployeeName = a.Employee.FullName,
                    Date = a.Date,
                    Clock_In = a.Clock_In,
                    Clock_Out = a.Clock_Out,
                    TotalHours = a.TotalHours,
                    OT_Hours = a.OT_Hours,
                    Status = a.Status.ToString()
                })
                .Take(500)
                .ToListAsync();
        }

        public async Task<Attendance?> GetTodayAttendanceAsync(int employeeId)
        {
            var today = DateTime.UtcNow.Date;
            return await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == today);
        }

        public async Task<IEnumerable<AttendanceListViewModel>> GetTodayAttendancesAsync()
        {
            var today = DateTime.UtcNow.Date;
            
            return await _context.Attendances
                .Where(a => a.Date == today)
                .Include(a => a.Employee)
                .Select(a => new AttendanceListViewModel
                {
                    Id = a.Id,
                    Emp_ID = a.EmployeeId.ToString(),
                    EmployeeName = a.Employee.FullName,
                    Date = a.Date,
                    Clock_In = a.Clock_In,
                    Clock_Out = a.Clock_Out,
                    Status = a.Status.ToString(),
                    TotalHours = a.TotalHours,
                    OT_Hours = a.OT_Hours
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

            var attendance = new Attendance
            {
                EmployeeId = employeeId,
                Date = date,
                Clock_In = TimeSpan.Zero,
                Status = AttendanceStatus.Absent,
                OT_Hours = 0,
                TotalHours = 0,
                CreatedAt = DateTime.UtcNow
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

            var today = DateTime.UtcNow.Date;
            var dayOfWeek = today.DayOfWeek;

            // Skip weekends
            if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
            {
                _logger.LogInformation("Today is a weekend, skipping absence check");
                return;
            }

            // Check if today is a public holiday
            var isHoliday = await _context.PublicHolidays
                .AnyAsync(ph => ph.HolidayDate.Date == today && ph.IsActive);

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
