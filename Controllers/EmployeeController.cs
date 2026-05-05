using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HR_Management_System.Models.ViewModels;
using HR_Management_System.Services.Interfaces;

namespace HR_Management_System.Controllers
{
    [Authorize(Roles = "Admin,HRManager")]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(IEmployeeService employeeService, ILogger<EmployeeController> logger)
        {
            _employeeService = employeeService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEmployees()
        {
            try
            {
                var employees = await _employeeService.GetAllEmployeesAsync();
                return Ok(employees);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all employees");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployeeById(int id)
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(id);
                if (employee == null)
                    return NotFound($"Employee with ID {id} not found");

                return Ok(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching employee with ID: {EmployeeId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromBody] EmployeeCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var employee = await _employeeService.CreateEmployeeAsync(model);
                return CreatedAtAction(nameof(GetEmployeeById), new { id = employee.Id }, employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating employee");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] EmployeeEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (id != model.Id)
                    return BadRequest("ID mismatch");

                var employee = await _employeeService.UpdateEmployeeAsync(model);
                return Ok(employee);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Employee not found with ID: {EmployeeId}", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating employee with ID: {EmployeeId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeactivateEmployee(int id, [FromQuery] string reason)
        {
            try
            {
                if (string.IsNullOrEmpty(reason))
                    return BadRequest("Termination reason is required");

                var result = await _employeeService.DeactivateEmployeeAsync(id, reason);
                if (!result)
                    return NotFound($"Employee with ID {id} not found");

                return Ok(new { message = "Employee deactivated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating employee with ID: {EmployeeId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPatch("{id}/activate")]
        public async Task<IActionResult> ActivateEmployee(int id)
        {
            try
            {
                var result = await _employeeService.ActivateEmployeeAsync(id);
                if (!result)
                    return NotFound($"Employee with ID {id} not found");

                return Ok(new { message = "Employee activated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating employee with ID: {EmployeeId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("probation")]
        public async Task<IActionResult> GetEmployeesOnProbation()
        {
            try
            {
                var employees = await _employeeService.GetEmployeesOnProbationAsync();
                return Ok(employees);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching employees on probation");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("probation/upcoming-expiries")]
        public async Task<IActionResult> GetUpcomingProbationExpiries([FromQuery] int daysAhead = 30)
        {
            try
            {
                var expiries = await _employeeService.GetUpcomingProbationExpiriesAsync(daysAhead);
                return Ok(expiries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching upcoming probation expiries");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{id}/probation/complete")]
        public async Task<IActionResult> CompleteProbation(int id)
        {
            try
            {
                var result = await _employeeService.CompleteProbationAsync(id);
                if (!result)
                    return NotFound($"Employee with ID {id} not found");

                return Ok(new { message = "Probation completed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing probation for employee ID: {EmployeeId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{id}/probation/extend")]
        public async Task<IActionResult> ExtendProbation(int id, [FromBody] DateTime newEndDate)
        {
            try
            {
                var result = await _employeeService.ExtendProbationAsync(id, newEndDate);
                if (!result)
                    return NotFound($"Employee with ID {id} not found");

                return Ok(new { message = "Probation extended successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extending probation for employee ID: {EmployeeId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchEmployees([FromQuery] string term)
        {
            try
            {
                if (string.IsNullOrEmpty(term))
                    return BadRequest("Search term is required");

                var employees = await _employeeService.SearchEmployeesAsync(term);
                return Ok(employees);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching employees with term: {SearchTerm}", term);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetEmployeeStats()
        {
            try
            {
                var totalActive = await _employeeService.GetTotalActiveEmployeesCountAsync();
                var onProbation = await _employeeService.GetEmployeesOnProbationCountAsync();

                return Ok(new
                {
                    TotalActiveEmployees = totalActive,
                    EmployeesOnProbation = onProbation
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching employee statistics");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}