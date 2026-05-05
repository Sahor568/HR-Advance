using HR_Management_System.Models;
using HR_Management_System.Models.Performance;
using HR_Management_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace HR_Management_System.Controllers
{
    [Authorize]
    public class PerformanceController : Controller
    {
        private readonly IPerformanceService _performanceService;
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<PerformanceController> _logger;

        public PerformanceController(
            IPerformanceService performanceService,
            IEmployeeService employeeService,
            ILogger<PerformanceController> logger)
        {
            _performanceService = performanceService;
            _employeeService = employeeService;
            _logger = logger;
        }

        // GET: Performance/Index
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var reviews = await _performanceService.GetAllPerformanceReviewsAsync();
                return View(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving performance reviews");
                TempData["ErrorMessage"] = "Failed to load performance reviews.";
                return View(new List<PerformanceReview>());
            }
        }

        // GET: Performance/Details/5
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var review = await _performanceService.GetPerformanceReviewByIdAsync(id);
                if (review == null)
                {
                    return NotFound();
                }
                return View(review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving performance review with ID {id}");
                TempData["ErrorMessage"] = "Failed to load performance review details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Performance/Create
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Employees = await _employeeService.GetAllEmployeesAsync();
            return View();
        }

        // POST: Performance/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> Create(PerformanceReview review)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _performanceService.CreatePerformanceReviewAsync(review);
                    _logger.LogInformation("Created performance review for employee {EmployeeId} by user {User}", review.EmployeeId, User.Identity.Name);
                    
                    TempData["SuccessMessage"] = "Performance review created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                ViewBag.Employees = await _employeeService.GetAllEmployeesAsync();
                return View(review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating performance review");
                TempData["ErrorMessage"] = "Failed to create performance review.";
                ViewBag.Employees = await _employeeService.GetAllEmployeesAsync();
                return View(review);
            }
        }

        // GET: Performance/Edit/5
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var review = await _performanceService.GetPerformanceReviewByIdAsync(id);
                if (review == null)
                {
                    return NotFound();
                }
                ViewBag.Employees = await _employeeService.GetAllEmployeesAsync();
                return View(review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving performance review for edit with ID {id}");
                TempData["ErrorMessage"] = "Failed to load performance review for editing.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Performance/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> Edit(int id, PerformanceReview review)
        {
            if (id != review.Id)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    await _performanceService.UpdatePerformanceReviewAsync(id, review);
                    _logger.LogInformation("Updated performance review {ReviewId} for employee {EmployeeId} by user {User}", id, review.EmployeeId, User.Identity.Name);
                    
                    TempData["SuccessMessage"] = "Performance review updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                ViewBag.Employees = await _employeeService.GetAllEmployeesAsync();
                return View(review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating performance review with ID {id}");
                TempData["ErrorMessage"] = "Failed to update performance review.";
                ViewBag.Employees = await _employeeService.GetAllEmployeesAsync();
                return View(review);
            }
        }

        // GET: Performance/Delete/5
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var review = await _performanceService.GetPerformanceReviewByIdAsync(id);
                if (review == null)
                {
                    return NotFound();
                }
                return View(review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving performance review for delete with ID {id}");
                TempData["ErrorMessage"] = "Failed to load performance review for deletion.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Performance/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _performanceService.DeletePerformanceReviewAsync(id);
                _logger.LogInformation("Deleted performance review {ReviewId} by user {User}", id, User.Identity.Name);
                
                TempData["SuccessMessage"] = "Performance review deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting performance review with ID {id}");
                TempData["ErrorMessage"] = "Failed to delete performance review.";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        // GET: Performance/EmployeeReviews/5
        [Authorize(Policy = "EmployeeOnly")]
        public async Task<IActionResult> EmployeeReviews(int employeeId)
        {
            try
            {
                var reviews = await _performanceService.GetPerformanceReviewsByEmployeeAsync(employeeId);
                return View(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving performance reviews for employee {employeeId}");
                TempData["ErrorMessage"] = "Failed to load your performance reviews.";
                return View(new List<PerformanceReview>());
            }
        }

        // GET: Performance/Goals/5
        public async Task<IActionResult> Goals(int employeeId)
        {
            try
            {
                var goals = await _performanceService.GetPerformanceGoalsByEmployeeAsync(employeeId);
                return View(goals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving performance goals for employee {employeeId}");
                TempData["ErrorMessage"] = "Failed to load performance goals.";
                return View(new List<PerformanceGoal>());
            }
        }

        // GET: Performance/CreateGoal
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> CreateGoal()
        {
            ViewBag.Employees = await _employeeService.GetAllEmployeesAsync();
            return View();
        }

        // POST: Performance/CreateGoal
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> CreateGoal(PerformanceGoal goal)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _performanceService.CreatePerformanceGoalAsync(goal);
                    _logger.LogInformation("Created performance goal for employee {EmployeeId} by user {User}", goal.EmployeeId, User.Identity.Name);
                    
                    TempData["SuccessMessage"] = "Performance goal created successfully.";
                    return RedirectToAction(nameof(Goals), new { employeeId = goal.EmployeeId });
                }
                ViewBag.Employees = await _employeeService.GetAllEmployeesAsync();
                return View(goal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating performance goal");
                TempData["ErrorMessage"] = "Failed to create performance goal.";
                ViewBag.Employees = await _employeeService.GetAllEmployeesAsync();
                return View(goal);
            }
        }

        // GET: Performance/OnlineExams
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> OnlineExams()
        {
            try
            {
                var exams = await _performanceService.GetAllOnlineExamsAsync();
                return View(exams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving online exams");
                TempData["ErrorMessage"] = "Failed to load online exams.";
                return View(new List<OnlineExam>());
            }
        }

        // GET: Performance/CreateOnlineExam
        [Authorize(Policy = "AdminOrHR")]
        public IActionResult CreateOnlineExam()
        {
            return View();
        }

        // POST: Performance/CreateOnlineExam
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> CreateOnlineExam(OnlineExam exam)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _performanceService.CreateOnlineExamAsync(exam);
                    _logger.LogInformation("Created online exam by user {User}", User.Identity.Name);
                    
                    TempData["SuccessMessage"] = "Online exam created successfully.";
                    return RedirectToAction(nameof(OnlineExams));
                }
                return View(exam);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating online exam");
                TempData["ErrorMessage"] = "Failed to create online exam.";
                return View(exam);
            }
        }

        // GET: Performance/TakeExam/5
        [Authorize(Policy = "EmployeeOnly")]
        public async Task<IActionResult> TakeExam(int id)
        {
            try
            {
                var exam = await _performanceService.GetOnlineExamByIdAsync(id);
                if (exam == null)
                {
                    return NotFound();
                }
                return View(exam);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving online exam with ID {id}");
                TempData["ErrorMessage"] = "Failed to load exam.";
                return RedirectToAction(nameof(OnlineExams));
            }
        }
    }
}