using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HR_Management_System.Models;
using HR_Management_System.Models.ViewModels;
using HR_Management_System.Services.Interfaces;

namespace HR_Management_System.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveController : ControllerBase
    {
        private readonly ILeaveService _leaveService;
        private readonly ILogger<LeaveController> _logger;

        public LeaveController(ILeaveService leaveService, ILogger<LeaveController> logger)
        {
            _leaveService = leaveService;
            _logger = logger;
        }

        [HttpPost("request")]
        public async Task<IActionResult> SubmitLeaveRequest([FromBody] LeaveRequestViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var leaveRequest = await _leaveService.SubmitLeaveRequestAsync(model);
                return Ok(new
                {
                    message = "Leave request submitted successfully",
                    requestId = leaveRequest.Id,
                    status = leaveRequest.Status.ToString()
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Employee not found for leave request");
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Leave request validation failed");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting leave request");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("approve")]
        [Authorize(Roles = "Admin,HRManager")]
        public async Task<IActionResult> ApproveLeaveRequest([FromBody] LeaveApprovalViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var user = User.Identity?.Name;
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

                var result = await _leaveService.ApproveLeaveRequestAsync(model, userId);
                if (!result)
                    return NotFound("Leave request not found or not in pending status");

                return Ok(new { message = "Leave request approved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving leave request ID: {RequestId}", model.LeaveRequestId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("reject/{id}")]
        [Authorize(Roles = "Admin,HRManager")]
        public async Task<IActionResult> RejectLeaveRequest(int id, [FromQuery] string reason)
        {
            try
            {
                if (string.IsNullOrEmpty(reason))
                    return BadRequest("Rejection reason is required");

                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

                var result = await _leaveService.RejectLeaveRequestAsync(id, userId, reason);
                if (!result)
                    return NotFound("Leave request not found or not in pending status");

                return Ok(new { message = "Leave request rejected" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting leave request ID: {RequestId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("pending")]
        [Authorize(Roles = "Admin,HRManager")]
        public async Task<IActionResult> GetPendingLeaveRequests()
        {
            try
            {
                var requests = await _leaveService.GetPendingLeaveRequestsAsync();
                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pending leave requests");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetEmployeeLeaveRequests(int employeeId)
        {
            try
            {
                var requests = await _leaveService.GetEmployeeLeaveRequestsAsync(employeeId);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching leave requests for employee ID: {EmployeeId}", employeeId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("balance/{employeeId}")]
        public async Task<IActionResult> GetLeaveBalance(int employeeId, [FromQuery] int? year)
        {
            try
            {
                var currentYear = year ?? DateTime.UtcNow.Year;
                var balance = await _leaveService.GetLeaveBalanceAsync(employeeId, currentYear);
                return Ok(balance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching leave balance for employee ID: {EmployeeId}", employeeId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("encashment/calculate")]
        public async Task<IActionResult> CalculateLeaveEncashment([FromBody] LeaveEncashmentViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var amount = await _leaveService.CalculateLeaveEncashmentAsync(model.EmployeeId, model.LeaveType, model.DaysToEncash);
                return Ok(new
                {
                    encashmentAmount = amount,
                    daysToEncash = model.DaysToEncash,
                    leaveType = model.LeaveType
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Employee not found for leave encashment calculation");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating leave encashment");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("encashment/process")]
        [Authorize(Roles = "Admin,HRManager")]
        public async Task<IActionResult> ProcessLeaveEncashment([FromBody] LeaveEncashmentViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _leaveService.ProcessLeaveEncashmentAsync(model);
                if (!result.Success)
                    return BadRequest(result.Message);

                return Ok(new
                {
                    message = result.Message,
                    encashmentAmount = result.EncashmentAmount,
                    daysEncashed = result.DaysEncashed,
                    leaveType = result.LeaveType
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing leave encashment");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("home-leave/accrue")]
        [Authorize(Roles = "Admin,HRManager")]
        public async Task<IActionResult> AccrueHomeLeave([FromQuery] int employeeId, [FromQuery] int workingDays)
        {
            try
            {
                var result = await _leaveService.AccrueHomeLeaveAsync(employeeId, workingDays);
                if (!result)
                    return BadRequest("Unable to accrue home leave");

                return Ok(new { message = "Home leave accrued successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accruing home leave for employee ID: {EmployeeId}", employeeId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("holidays")]
        public async Task<IActionResult> GetPublicHolidays([FromQuery] int? year)
        {
            try
            {
                var currentYear = year ?? DateTime.UtcNow.Year;
                var holidays = await _leaveService.GetPublicHolidaysAsync(currentYear);
                return Ok(holidays);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching public holidays for year: {Year}", year);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("holidays")]
        [Authorize(Roles = "Admin,HRManager")]
        public async Task<IActionResult> CreatePublicHoliday([FromBody] PublicHoliday holiday)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdHoliday = await _leaveService.AddPublicHolidayAsync(holiday);
                return CreatedAtAction(nameof(GetPublicHolidays), new { year = holiday.HolidayDate.Year }, createdHoliday);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating public holiday");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("holidays/{id}")]
        [Authorize(Roles = "Admin,HRManager")]
        public async Task<IActionResult> UpdatePublicHoliday(int id, [FromBody] PublicHoliday holiday)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != holiday.Id)
                {
                    return BadRequest("ID mismatch");
                }

                var updatedHoliday = await _leaveService.UpdatePublicHolidayAsync(id, holiday);
                return Ok(updatedHoliday);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Public holiday not found: {Id}", id);
                return NotFound($"Public holiday with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating public holiday");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("holidays/{id}")]
        [Authorize(Roles = "Admin,HRManager")]
        public async Task<IActionResult> DeletePublicHoliday(int id)
        {
            try
            {
                var result = await _leaveService.DeletePublicHolidayAsync(id);
                if (!result)
                {
                    return NotFound($"Public holiday with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting public holiday");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("stats/pending-count")]
        [Authorize(Roles = "Admin,HRManager")]
        public async Task<IActionResult> GetPendingLeaveCount()
        {
            try
            {
                var count = await _leaveService.GetPendingLeaveCountAsync();
                return Ok(new { pendingLeaveCount = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pending leave count");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}