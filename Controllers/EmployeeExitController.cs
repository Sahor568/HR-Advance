using HR_Management_System.Models;
using HR_Management_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace HR_Management_System.Controllers
{
    [Authorize]
    public class EmployeeExitController : Controller
    {
        private readonly IEmployeeExitService _employeeExitService;
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<EmployeeExitController> _logger;

        public EmployeeExitController(
            IEmployeeExitService employeeExitService,
            IEmployeeService employeeService,
            ILogger<EmployeeExitController> logger)
        {
            _employeeExitService = employeeExitService;
            _employeeService = employeeService;
            _logger = logger;
        }

        // GET: EmployeeExit/Index
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var exits = await _employeeExitService.GetAllEmployeeExitsAsync();
                return View(exits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee exits");
                TempData["ErrorMessage"] = "Failed to load employee exits.";
                return View(new List<EmployeeExit>());
            }
        }

        // GET: EmployeeExit/Details/5
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var exit = await _employeeExitService.GetEmployeeExitByIdAsync(id);
                if (exit == null)
                {
                    return NotFound();
                }
                return View(exit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving employee exit with ID {id}");
                TempData["ErrorMessage"] = "Failed to load employee exit details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: EmployeeExit/Create
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Employees = await _employeeService.GetAllEmployeesAsync();
            return View();
        }

        // POST: EmployeeExit/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> Create(EmployeeExit exit)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _employeeExitService.CreateEmployeeExitAsync(exit);
                    _logger.LogInformation("Created employee exit for employee {EmployeeId} by user {User}", 
                        exit.EmployeeId, User.Identity.Name);
                    
                    TempData["SuccessMessage"] = "Employee exit record created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                ViewBag.Employees = await _employeeService.GetAllEmployeesAsync();
                return View(exit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating employee exit");
                TempData["ErrorMessage"] = "Failed to create employee exit record.";
                ViewBag.Employees = await _employeeService.GetAllEmployeesAsync();
                return View(exit);
            }
        }

        // GET: EmployeeExit/Edit/5
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var exit = await _employeeExitService.GetEmployeeExitByIdAsync(id);
                if (exit == null)
                {
                    return NotFound();
                }
                ViewBag.Employees = await _employeeService.GetAllEmployeesAsync();
                return View(exit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving employee exit for edit with ID {id}");
                TempData["ErrorMessage"] = "Failed to load employee exit for editing.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: EmployeeExit/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> Edit(int id, EmployeeExit exit)
        {
            if (id != exit.Id)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    await _employeeExitService.UpdateEmployeeExitAsync(id, exit);
                    _logger.LogInformation("Updated employee exit {ExitId} for employee {EmployeeId} by user {User}", 
                        id, exit.EmployeeId, User.Identity.Name);
                    
                    TempData["SuccessMessage"] = "Employee exit record updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                ViewBag.Employees = await _employeeService.GetAllEmployeesAsync();
                return View(exit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating employee exit with ID {id}");
                TempData["ErrorMessage"] = "Failed to update employee exit record.";
                ViewBag.Employees = await _employeeService.GetAllEmployeesAsync();
                return View(exit);
            }
        }

        // GET: EmployeeExit/Delete/5
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var exit = await _employeeExitService.GetEmployeeExitByIdAsync(id);
                if (exit == null)
                {
                    return NotFound();
                }
                return View(exit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving employee exit for delete with ID {id}");
                TempData["ErrorMessage"] = "Failed to load employee exit for deletion.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: EmployeeExit/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _employeeExitService.DeleteEmployeeExitAsync(id);
                _logger.LogInformation("Deleted employee exit {ExitId} by user {User}", id, User.Identity.Name);
                
                TempData["SuccessMessage"] = "Employee exit record deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting employee exit with ID {id}");
                TempData["ErrorMessage"] = "Failed to delete employee exit record.";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        // GET: EmployeeExit/ResignationRequests
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> ResignationRequests()
        {
            try
            {
                var requests = await _employeeExitService.GetPendingResignationRequestsAsync();
                return View(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving resignation requests");
                TempData["ErrorMessage"] = "Failed to load resignation requests.";
                return View(new List<EmployeeExit>());
            }
        }

        // GET: EmployeeExit/ApproveResignation/5
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> ApproveResignation(int id)
        {
            try
            {
                var exit = await _employeeExitService.GetEmployeeExitByIdAsync(id);
                if (exit == null)
                {
                    return NotFound();
                }
                return View(exit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving resignation for approval with ID {id}");
                TempData["ErrorMessage"] = "Failed to load resignation for approval.";
                return RedirectToAction(nameof(ResignationRequests));
            }
        }

        // POST: EmployeeExit/ApproveResignation/5
        [HttpPost, ActionName("ApproveResignation")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> ApproveResignationConfirmed(int id)
        {
            try
            {
                await _employeeExitService.ApproveResignationAsync(id, User.Identity.Name);
                _logger.LogInformation("Approved resignation {ExitId} by user {User}", id, User.Identity.Name);
                
                TempData["SuccessMessage"] = "Resignation approved successfully.";
                return RedirectToAction(nameof(ResignationRequests));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error approving resignation with ID {id}");
                TempData["ErrorMessage"] = "Failed to approve resignation.";
                return RedirectToAction(nameof(ApproveResignation), new { id });
            }
        }

        // GET: EmployeeExit/RejectResignation/5
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> RejectResignation(int id)
        {
            try
            {
                var exit = await _employeeExitService.GetEmployeeExitByIdAsync(id);
                if (exit == null)
                {
                    return NotFound();
                }
                return View(exit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving resignation for rejection with ID {id}");
                TempData["ErrorMessage"] = "Failed to load resignation for rejection.";
                return RedirectToAction(nameof(ResignationRequests));
            }
        }

        // POST: EmployeeExit/RejectResignation/5
        [HttpPost, ActionName("RejectResignation")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> RejectResignationConfirmed(int id, string rejectionReason)
        {
            try
            {
                await _employeeExitService.RejectResignationAsync(id, rejectionReason, User.Identity.Name);
                _logger.LogInformation("Rejected resignation {ExitId} by user {User}", id, User.Identity.Name);
                
                TempData["SuccessMessage"] = "Resignation rejected successfully.";
                return RedirectToAction(nameof(ResignationRequests));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error rejecting resignation with ID {id}");
                TempData["ErrorMessage"] = "Failed to reject resignation.";
                return RedirectToAction(nameof(RejectResignation), new { id });
            }
        }

        // GET: EmployeeExit/ExitClearance/5
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> ExitClearance(int id)
        {
            try
            {
                var clearances = await _employeeExitService.GetExitClearancesByExitIdAsync(id);
                ViewBag.ExitId = id;
                return View(clearances);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving exit clearances for exit ID {id}");
                TempData["ErrorMessage"] = "Failed to load exit clearances.";
                return View(new List<ExitClearance>());
            }
        }

        // GET: EmployeeExit/CreateExitClearance/5
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> CreateExitClearance(int exitId)
        {
            ViewBag.ExitId = exitId;
            return View();
        }

        // POST: EmployeeExit/CreateExitClearance
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> CreateExitClearance(ExitClearance clearance)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _employeeExitService.CreateExitClearanceAsync(clearance);
                    _logger.LogInformation("Created exit clearance for exit {ExitId} by user {User}", 
                        clearance.EmployeeExitId, User.Identity.Name);
                    
                    TempData["SuccessMessage"] = "Exit clearance created successfully.";
                    return RedirectToAction(nameof(ExitClearance), new { id = clearance.EmployeeExitId });
                }
                ViewBag.ExitId = clearance.EmployeeExitId;
                return View(clearance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating exit clearance");
                TempData["ErrorMessage"] = "Failed to create exit clearance.";
                ViewBag.ExitId = clearance.EmployeeExitId;
                return View(clearance);
            }
        }

        // GET: EmployeeExit/ExitSurveys
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> ExitSurveys()
        {
            try
            {
                var surveys = await _employeeExitService.GetAllExitSurveysAsync();
                return View(surveys);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving exit surveys");
                TempData["ErrorMessage"] = "Failed to load exit surveys.";
                return View(new List<ExitSurvey>());
            }
        }

        // GET: EmployeeExit/ExitSurveyDetails/5
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> ExitSurveyDetails(int id)
        {
            try
            {
                var survey = await _employeeExitService.GetExitSurveyByIdAsync(id);
                if (survey == null)
                {
                    return NotFound();
                }
                return View(survey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving exit survey with ID {id}");
                TempData["ErrorMessage"] = "Failed to load exit survey details.";
                return RedirectToAction(nameof(ExitSurveys));
            }
        }

        // GET: EmployeeExit/FinalSettlement/5
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> FinalSettlement(int id)
        {
            try
            {
                var settlement = await _employeeExitService.CalculateFinalSettlementAsync(id);
                ViewBag.ExitId = id;
                return View(settlement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calculating final settlement for exit ID {id}");
                TempData["ErrorMessage"] = "Failed to calculate final settlement.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // GET: EmployeeExit/SubmitResignation (Employee Self-Service)
        [Authorize(Policy = "EmployeeOnly")]
        public async Task<IActionResult> SubmitResignation()
        {
            var employee = await _employeeService.GetEmployeeByUserIdAsync(User.Identity.Name);
            if (employee == null)
            {
                return NotFound();
            }
            
            ViewBag.EmployeeId = employee.Id;
            return View();
        }

        // POST: EmployeeExit/SubmitResignation
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EmployeeOnly")]
        public async Task<IActionResult> SubmitResignation(EmployeeExit resignation)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var employee = await _employeeService.GetEmployeeByUserIdAsync(User.Identity.Name);
                    if (employee == null)
                    {
                        return NotFound();
                    }
                    
                    resignation.EmployeeId = employee.Id;
                    resignation.Type = Models.ExitType.Resignation;
                    resignation.Status = Models.ExitStatus.ResignationSubmitted;
                    
                    await _employeeExitService.CreateEmployeeExitAsync(resignation);
                    _logger.LogInformation("Employee {EmployeeId} submitted resignation", employee.Id);
                    
                    TempData["SuccessMessage"] = "Your resignation has been submitted successfully.";
                    return RedirectToAction(nameof(MyResignationStatus));
                }
                var emp = await _employeeService.GetEmployeeByUserIdAsync(User.Identity.Name);
                ViewBag.EmployeeId = emp?.Id;
                return View(resignation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting resignation");
                TempData["ErrorMessage"] = "Failed to submit resignation.";
                var emp = await _employeeService.GetEmployeeByUserIdAsync(User.Identity.Name);
                ViewBag.EmployeeId = emp?.Id;
                return View(resignation);
            }
        }

        // GET: EmployeeExit/MyResignationStatus (Employee Self-Service)
        [Authorize(Policy = "EmployeeOnly")]
        public async Task<IActionResult> MyResignationStatus()
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByUserIdAsync(User.Identity.Name);
                if (employee == null)
                {
                    return NotFound();
                }
                
                var exits = await _employeeExitService.GetEmployeeExitsByEmployeeIdAsync(employee.Id);
                return View(exits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving resignation status");
                TempData["ErrorMessage"] = "Failed to load your resignation status.";
                return View(new List<EmployeeExit>());
            }
        }
    }
}