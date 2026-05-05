using HR_Management_System.Models;
using HR_Management_System.Models.ViewModels;

namespace HR_Management_System.Services.Interfaces
{
    public interface IAttendanceService
    {
        Task<Attendance> ClockInAsync(int employeeId);
        Task<Attendance> ClockOutAsync(int employeeId);
        Task<IEnumerable<AttendanceListViewModel>> GetAttendanceByDateRangeAsync(AttendanceFilterViewModel filter);
        Task<Attendance?> GetTodayAttendanceAsync(int employeeId);
        Task<IEnumerable<WeeklyWorkHoursViewModel>> GetWeeklyWorkHoursAsync(int employeeId, DateTime? weekStart = null);
        Task<bool> MarkAbsentAsync(int employeeId, DateTime date);
        Task<decimal> CalculateOvertimeAsync(int employeeId, DateTime date);
        Task<Dictionary<int, decimal>> GetMonthlyOTSummaryAsync(int month, int year);
        Task CheckAndMarkAbsencesAsync();
    }
}
