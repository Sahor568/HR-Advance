using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HR_Management_System.Services.Interfaces;

namespace HR_Management_System.Controllers
{
    [Authorize(Roles = "Admin,HRManager,Employee")]
    [Route("api/[controller]")]
    [ApiController]
    public class SSFController : ControllerBase
    {
        private readonly ISSFService _ssfService;
        private readonly ILogger<SSFController> _logger;

        public SSFController(ISSFService ssfService, ILogger<SSFController> logger)
        {
            _ssfService = ssfService;
            _logger = logger;
        }

        [HttpGet("balance/{employeeId}")]
        public async Task<IActionResult> GetSSFBalance(int employeeId)
        {
            try
            {
                var balance = await _ssfService.GetSSFBalanceAsync(employeeId);
                return Ok(balance);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Employee not found for SSF balance: {EmployeeId}", employeeId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching SSF balance for employee ID: {EmployeeId}", employeeId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("history/{employeeId}")]
        public async Task<IActionResult> GetSSFHistory(int employeeId)
        {
            try
            {
                var history = await _ssfService.GetSSFHistoryAsync(employeeId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching SSF history for employee ID: {EmployeeId}", employeeId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("contributions/employee/{employeeId}")]
        public async Task<IActionResult> GetEmployeeContribution(int employeeId)
        {
            try
            {
                var contribution = await _ssfService.GetTotalEmployeeContributionAsync(employeeId);
                return Ok(new
                {
                    employeeId,
                    totalEmployeeContribution = contribution
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching employee contribution for employee ID: {EmployeeId}", employeeId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("contributions/employer/{employeeId}")]
        public async Task<IActionResult> GetEmployerContribution(int employeeId)
        {
            try
            {
                var contribution = await _ssfService.GetTotalEmployerContributionAsync(employeeId);
                return Ok(new
                {
                    employeeId,
                    totalEmployerContribution = contribution
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching employer contribution for employee ID: {EmployeeId}", employeeId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}