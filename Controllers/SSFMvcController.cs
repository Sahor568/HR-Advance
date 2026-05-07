using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HR_Management_System.Services.Interfaces;

namespace HR_Management_System.Controllers
{
    [Authorize(Roles = "Admin,HRManager,Employee")]
    [Route("SSF")]
    public class SSFMvcController : Controller
    {
        private readonly ISSFService _ssfService;
        private readonly ILogger<SSFMvcController> _logger;

        public SSFMvcController(ISSFService ssfService, ILogger<SSFMvcController> logger)
        {
            _ssfService = ssfService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("balance")]
        public async Task<IActionResult> Balance(int employeeId)
        {
            try
            {
                // If no employeeId provided, use current user's employee ID
                if (employeeId == 0)
                {
                    // In a real implementation, you'd get the employee ID from the current user
                    // For now, we'll show a form to enter employee ID
                    return View("BalanceForm");
                }

                var balance = await _ssfService.GetSSFBalanceAsync(employeeId);
                ViewBag.EmployeeId = employeeId;
                return View("BalanceResult", balance);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Employee not found for SSF balance: {EmployeeId}", employeeId);
                TempData["ErrorMessage"] = $"Employee with ID {employeeId} not found.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching SSF balance for employee ID: {EmployeeId}", employeeId);
                TempData["ErrorMessage"] = "An error occurred while fetching SSF balance.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> History(int employeeId)
        {
            try
            {
                if (employeeId == 0)
                {
                    return View("HistoryForm");
                }

                var history = await _ssfService.GetSSFHistoryAsync(employeeId);
                ViewBag.EmployeeId = employeeId;
                return View("HistoryResult", history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching SSF history for employee ID: {EmployeeId}", employeeId);
                TempData["ErrorMessage"] = "An error occurred while fetching SSF history.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet("contributions")]
        public async Task<IActionResult> Contributions(int employeeId)
        {
            try
            {
                if (employeeId == 0)
                {
                    return View("ContributionsForm");
                }

                var employeeContribution = await _ssfService.GetTotalEmployeeContributionAsync(employeeId);
                var employerContribution = await _ssfService.GetTotalEmployerContributionAsync(employeeId);
                
                ViewBag.EmployeeId = employeeId;
                ViewBag.EmployeeContribution = employeeContribution;
                ViewBag.EmployerContribution = employerContribution;
                ViewBag.TotalContribution = employeeContribution + employerContribution;
                
                return View("ContributionsResult");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching SSF contributions for employee ID: {EmployeeId}", employeeId);
                TempData["ErrorMessage"] = "An error occurred while fetching SSF contributions.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}