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

                // Validate location
                if (!model.IsLocationEnabled)
                {
                    return BadRequest("Location must be enabled to check-in. Please enable location services.");
                }

                if (!model.ValidateLocation())
                {
                    return BadRequest("Valid location coordinates or address is required for check-in.");
                }

                var attendance = await _attendanceService.ClockInAsync(
                    model.EmployeeId,
                    model.Latitude,
                    model.Longitude,
                    model.LocationAddress);
                
                return Ok(new
                {
                    message = "Clock-in successful",
                    attendanceId = attendance.Id,
                    clockInTime = attendance.Clock_In,
                    date = attendance.Date,
                    locationRecorded = attendance.Latitude != null && attendance.Longitude != null
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

        [HttpGet("today")]
        [Authorize(Roles = "Admin,HRManager")]
        public async Task<IActionResult> GetTodayAttendances()
        {
            try
            {
                var attendances = await _attendanceService.GetTodayAttendancesAsync();
                return Ok(attendances);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching today's attendances");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("today-count")]
        public async Task<IActionResult> GetTodayAttendanceCount()
        {
            try
            {
                var attendances = await _attendanceService.GetTodayAttendancesAsync();
                return Ok(new { count = attendances.Count() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching today's attendance count");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
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

        [HttpGet("export")]
        public async Task<IActionResult> ExportAttendance([FromQuery] AttendanceFilterViewModel filter)
        {
            try
            {
                var data = await _attendanceService.GetAttendanceByDateRangeAsync(filter);
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Date,Nepali Date,Employee ID,Employee Name,Clock In,Clock Out,Total Hours,OT Hours,Status,IsHoliday,Holiday Name,IsWeekend,Location,Remarks");
                foreach (var a in data)
                {
                    var remarks = a.Remarks?.Replace(",", ";").Replace("\n", " ") ?? "";
                    var location = a.LocationAddress?.Replace(",", ";").Replace("\n", " ") ?? "";
                    var clockIn = a.Clock_In != TimeSpan.Zero ? a.Clock_In.ToString(@"hh\:mm") : "";
                    var clockOut = a.Clock_Out.HasValue && a.Clock_Out.Value != TimeSpan.Zero ? a.Clock_Out.Value.ToString(@"hh\:mm") : "";
                    
                    sb.AppendLine($"{a.Date:yyyy-MM-dd},{a.NepaliDate},{a.Emp_ID},{a.EmployeeName},{clockIn},{clockOut},{a.TotalHours},{a.OT_Hours},{a.Status},{(a.IsHoliday ? "Yes" : "No")},{a.HolidayName ?? ""},{(a.IsWeekend ? "Yes" : "No")},{location},{remarks}");
                }
                return File(System.Text.Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"Attendance_{DateTime.UtcNow:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting attendance");
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

        [HttpGet("pending")]
        [Authorize(Roles = "Admin,HRManager")]
        public async Task<IActionResult> GetPendingAttendances([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var pendingAttendances = await _attendanceService.GetPendingAttendancesAsync(fromDate, toDate);
                return Ok(pendingAttendances);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pending attendance records");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("approve/{attendanceId}")]
        [Authorize(Roles = "Admin,HRManager")]
        public async Task<IActionResult> ApproveAttendance(int attendanceId, [FromBody] ApproveAttendanceViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _attendanceService.ApproveAttendanceAsync(attendanceId, model.ApprovedBy, model.Remarks);
                if (!result)
                    return BadRequest("Attendance could not be approved");

                return Ok(new { message = "Attendance approved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving attendance ID: {AttendanceId}", attendanceId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("reject/{attendanceId}")]
        [Authorize(Roles = "Admin,HRManager")]
        public async Task<IActionResult> RejectAttendance(int attendanceId, [FromBody] RejectAttendanceViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _attendanceService.RejectAttendanceAsync(attendanceId, model.RejectedBy, model.Remarks);
                if (!result)
                    return BadRequest("Attendance could not be rejected");

                return Ok(new { message = "Attendance rejected and marked as absent" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting attendance ID: {AttendanceId}", attendanceId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("is-holiday")]
        public async Task<IActionResult> IsTodayHoliday()
        {
            try
            {
                var result = await _attendanceService.IsTodayHolidayOrWeekendAsync();
                return Ok(new { isHoliday = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking holiday status");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}