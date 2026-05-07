using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HR_Management_System.Models.ViewModels;
using HR_Management_System.Services.Interfaces;

namespace HR_Management_System.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<AuditLogController> _logger;

        public AuditLogController(IAuditLogService auditLogService, ILogger<AuditLogController> logger)
        {
            _auditLogService = auditLogService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogFilterViewModel filter)
        {
            try
            {
                var logs = await _auditLogService.GetAuditLogsAsync(filter);
                return Ok(logs.Logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching audit logs");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuditLogById(int id)
        {
            try
            {
                var log = await _auditLogService.GetAuditLogByIdAsync(id);
                if (log == null)
                    return NotFound($"Audit log with ID {id} not found");

                return Ok(log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching audit log with ID: {LogId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentLogs([FromQuery] int count = 100)
        {
            try
            {
                var logs = await _auditLogService.GetRecentLogsAsync(count);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching recent audit logs");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}