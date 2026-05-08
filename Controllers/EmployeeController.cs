using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HR_Management_System.Models.ViewModels;
using HR_Management_System.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using HR_Management_System.Models.Identity;
using HR_Management_System.Models;

namespace HR_Management_System.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<EmployeeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployeeController(IEmployeeService employeeService, IFileUploadService fileUploadService, ILogger<EmployeeController> logger, UserManager<ApplicationUser> userManager)
        {
            _employeeService = employeeService;
            _fileUploadService = fileUploadService;
            _logger = logger;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin,HRManager")]
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
                // If not admin/HR, check if viewing own profile
                if (!User.IsInRole("Admin") && !User.IsInRole("HRManager"))
                {
                    var myEmp = await _employeeService.GetEmployeeByUserIdAsync(User.Identity.Name);
                    if (myEmp == null || myEmp.Id != id)
                        return Forbid();
                }

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

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogWarning("GetMyProfile: User is null");
                    return Unauthorized();
                }

                _logger.LogInformation("GetMyProfile: Loading for user {Email}, EmployeeId: {EmpId}", user.Email, user.EmployeeId);

                if (user.EmployeeId.HasValue)
                {
                    var detail = await _employeeService.GetEmployeeByIdAsync(user.EmployeeId.Value);
                    if (detail != null) 
                    {
                        _logger.LogInformation("GetMyProfile: Found linked employee {Id}", user.EmployeeId.Value);
                        return Ok(detail);
                    }
                }

                // Fallback to email lookup
                _logger.LogInformation("GetMyProfile: Trying fallback email lookup for {Email}", user.Email);
                var employee = await _employeeService.GetEmployeeByUserIdAsync(user.Email);
                if (employee != null)
                {
                    // Auto-fix the link if missing
                    if (!user.EmployeeId.HasValue)
                    {
                        user.EmployeeId = employee.Id;
                        await _userManager.UpdateAsync(user);
                        _logger.LogInformation("GetMyProfile: Auto-linked user {Email} to employee {Id}", user.Email, employee.Id);
                    }
                    
                    var detail = await _employeeService.GetEmployeeByIdAsync(employee.Id);
                    return Ok(detail);
                }

                _logger.LogWarning("GetMyProfile: No employee found for user {Email}", user.Email);
                return NotFound("Employee profile not found for the current user.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching current user profile");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "Admin,HRManager")]
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateEmployee([FromForm] EmployeeCreateViewModel model,
                                                       [FromForm] IFormFile? photoFile,
                                                       [FromForm] IFormFile? cvFile,
                                                       [FromForm] IFormFile? experienceCertificateFile)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Handle file uploads
                if (photoFile != null && photoFile.Length > 0)
                {
                    model.PhotoPath = await _fileUploadService.UploadFileAsync(photoFile, "employee-photos");
                }

                if (cvFile != null && cvFile.Length > 0)
                {
                    model.CVPath = await _fileUploadService.UploadFileAsync(cvFile, "employee-cvs");
                }

                if (experienceCertificateFile != null && experienceCertificateFile.Length > 0)
                {
                    model.ExperienceCertificatePath = await _fileUploadService.UploadFileAsync(experienceCertificateFile, "employee-certificates");
                }

                var employee = await _employeeService.CreateEmployeeAsync(model);
                return CreatedAtAction(nameof(GetEmployeeById), new { id = employee.Id }, employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating employee");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "Admin,HRManager")]
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateEmployee(int id,
                                                       [FromForm] EmployeeEditViewModel model,
                                                       [FromForm] IFormFile? photoFile,
                                                       [FromForm] IFormFile? cvFile,
                                                       [FromForm] IFormFile? experienceCertificateFile)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (id != model.Id)
                    return BadRequest("ID mismatch");

                // Handle file uploads
                if (photoFile != null && photoFile.Length > 0)
                {
                    model.PhotoPath = await _fileUploadService.UploadFileAsync(photoFile, "employee-photos");
                }

                if (cvFile != null && cvFile.Length > 0)
                {
                    model.CVPath = await _fileUploadService.UploadFileAsync(cvFile, "employee-cvs");
                }

                if (experienceCertificateFile != null && experienceCertificateFile.Length > 0)
                {
                    model.ExperienceCertificatePath = await _fileUploadService.UploadFileAsync(experienceCertificateFile, "employee-certificates");
                }

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

        [Authorize(Roles = "Admin,HRManager")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeactivateEmployee(int id, [FromQuery] string reason = "Deactivated by admin")
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

        [Authorize(Roles = "Admin,HRManager")]
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
                    EmployeesOnProbation = onProbation,
                    totalActive = totalActive,
                    onProbation = onProbation
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