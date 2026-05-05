using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HR_Management_System.Models.ViewModels;
using HR_Management_System.Services.Interfaces;

namespace HR_Management_System.Controllers
{
    [Authorize(Roles = "Admin,HRManager")]
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;
        private readonly ILogger<DepartmentsController> _logger;

        public DepartmentsController(IDepartmentService departmentService, ILogger<DepartmentsController> logger)
        {
            _departmentService = departmentService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDepartments()
        {
            try
            {
                var departments = await _departmentService.GetAllDepartmentsAsync();
                return Ok(departments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all departments");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveDepartments()
        {
            try
            {
                var departments = await _departmentService.GetActiveDepartmentsAsync();
                return Ok(departments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching active departments");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDepartmentById(int id)
        {
            try
            {
                var department = await _departmentService.GetDepartmentByIdAsync(id);
                if (department == null)
                    return NotFound($"Department with ID {id} not found");

                return Ok(department);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching department with ID: {DepartmentId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}/employee-count")]
        public async Task<IActionResult> GetDepartmentEmployeeCount(int id)
        {
            try
            {
                var count = await _departmentService.GetDepartmentEmployeeCountAsync(id);
                return Ok(new { departmentId = id, employeeCount = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching employee count for department ID: {DepartmentId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchDepartments([FromQuery] string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term))
                    return BadRequest("Search term is required");

                var departments = await _departmentService.SearchDepartmentsAsync(term);
                return Ok(departments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching departments with term: {SearchTerm}", term);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateDepartment([FromBody] DepartmentCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var department = await _departmentService.CreateDepartmentAsync(model);
                return CreatedAtAction(nameof(GetDepartmentById), new { id = department.Id }, department);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error creating department");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating department");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDepartment(int id, [FromBody] DepartmentEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (id != model.Id)
                    return BadRequest("Department ID mismatch");

                var department = await _departmentService.UpdateDepartmentAsync(model);
                return Ok(department);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Department not found for update");
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error updating department");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating department with ID: {DepartmentId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            try
            {
                var result = await _departmentService.DeleteDepartmentAsync(id);
                if (!result)
                    return NotFound($"Department with ID {id} not found");

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot delete department with active employees");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting department with ID: {DepartmentId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ToggleDepartmentStatus(int id, [FromBody] bool isActive)
        {
            try
            {
                var result = await _departmentService.ToggleDepartmentStatusAsync(id, isActive);
                if (!result)
                    return NotFound($"Department with ID {id} not found");

                return Ok(new { departmentId = id, isActive = isActive });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling department status for ID: {DepartmentId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}