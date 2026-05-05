using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HR_Management_System.Models.ViewModels;
using HR_Management_System.Services.Interfaces;

namespace HR_Management_System.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;
        private readonly ILogger<AttendanceController> _logger;

        public AttendanceController(IAttendanceService attendanceService, ILogger<AttendanceController> logger)
        {
            _attendanceService = attendanceService;
            _logger = logger;
        }

        [HttpPost("clock-in")]
        public async Task<IActionResult> ClockIn([FromBody] ClockInViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var attendance = await _attendanceService.ClockInAsync(model.EmployeeId);
                return Ok(new
                {
                    message = "Clock-in successful",
                    attendanceId = attendance.Id,
                    clockInTime = attendance.Clock_In,
                    date = attendance.Date
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Clock-in failed for employee ID: {EmployeeId}", model.EmployeeId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during clock-in for employee ID: {EmployeeId}", model.EmployeeId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("clock-out")]
        public async Task<IActionResult> ClockOut([FromBody] ClockOutViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var attendance = await _attendanceService.ClockOutAsync(model.EmployeeId);
                return Ok(new
                {
                    message = "Clock-out successful",
                    attendanceId = attendance.Id,
                    clockInTime = attendance.Clock_In,
                    clockOutTime = attendance.Clock_Out,
                    totalHours = attendance.TotalHours,
                    overtimeHours = attendance.OT_Hours
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Clock-out failed for employee ID: {EmployeeId}", model.EmployeeId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during clock-out for employee ID: {EmployeeId}", model.EmployeeId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("today/{employeeId}")]
        public async Task<IActionResult> GetTodayAttendance(int employeeId)
        {
            try
            {
                var attendance = await _attendanceService.GetTodayAttendanceAsync(employeeId);
                if (attendance == null)
                    return NotFound("No attendance record found for today");

                return Ok(attendance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching today's attendance for employee ID: {EmployeeId}", employeeId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,HRManager")]
        public async Task<IActionResult> GetAttendance([FromQuery] AttendanceFilterViewModel filter)
        {
            try
            {
                var attendance = await _attendanceService.GetAttendanceByDateRangeAsync(filter);
                return Ok(attendance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching attendance records");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("weekly-hours/{employeeId}")]
        public async Task<IActionResult> GetWeeklyWorkHours(int employeeId, [FromQuery] DateTime? weekStart)
        {
            try
            {
                var weeklyHours = await _attendanceService.GetWeeklyWorkHoursAsync(employeeId, weekStart);
                return Ok(weeklyHours);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching weekly work hours for employee ID: {EmployeeId}", employeeId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("mark-absent")]
        [Authorize(Roles = "Admin,HRManager")]
        public async Task<IActionResult> MarkAbsent([FromQuery] int employeeId, [FromQuery] DateTime date)
        {
            try
            {
                var result = await _attendanceService.MarkAbsentAsync(employeeId, date);
                if (!result)
                    return BadRequest("Attendance record already exists for this date");

                return Ok(new { message = "Employee marked as absent" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking absent for employee ID: {EmployeeId} on {Date}", employeeId, date);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("overtime/{employeeId}")]
        public async Task<IActionResult> GetOvertime(int employeeId, [FromQuery] DateTime date)
        {
            try
            {
                var overtime = await _attendanceService.CalculateOvertimeAsync(employeeId, date);
                return Ok(new { overtimeHours = overtime });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating overtime for employee ID: {EmployeeId} on {Date}", employeeId, date);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("overtime-summary")]
        [Authorize(Roles = "Admin,HRManager")]
        public async Task<IActionResult> GetMonthlyOTSummary([FromQuery] int month, [FromQuery] int year)
        {
            try
            {
                var summary = await _attendanceService.GetMonthlyOTSummaryAsync(month, year);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching monthly OT summary for {Month}/{Year}", month, year);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("check-absences")]
        [Authorize(Roles = "Admin,HRManager")]
        public async Task<IActionResult> CheckAndMarkAbsences()
        {
            try
            {
                await _attendanceService.CheckAndMarkAbsencesAsync();
                return Ok(new { message = "Absence check completed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking and marking absences");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}