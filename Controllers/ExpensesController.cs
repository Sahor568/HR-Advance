using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HR_Management_System.Services.Interfaces;
using Serilog;

namespace HR_Management_System.Controllers
{
    [Authorize]
    public class ExpensesController : Controller
    {
        private readonly IHRISService _hrisService;
        private readonly ILogger<ExpensesController> _logger;

        public ExpensesController(IHRISService hrisService, ILogger<ExpensesController> logger)
        {
            _hrisService = hrisService;
            _logger = logger;
        }

        // GET: Expenses/Index
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var travelRequests = await _hrisService.GetAllTravelRequestsAsync();
                var reimbursements = await _hrisService.GetAllReimbursementsAsync();
                
                ViewBag.TravelRequests = travelRequests;
                ViewBag.Reimbursements = reimbursements;
                
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading expenses");
                TempData["ErrorMessage"] = "Failed to load expense data.";
                return View();
            }
        }

        // GET: Expenses/Travel
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> Travel()
        {
            try
            {
                var travelRequests = await _hrisService.GetAllTravelRequestsAsync();
                return View(travelRequests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading travel requests");
                TempData["ErrorMessage"] = "Failed to load travel requests.";
                return View(new List<HR_Management_System.Models.TravelRequest>());
            }
        }

        // GET: Expenses/Reimbursements
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> Reimbursements()
        {
            try
            {
                var reimbursements = await _hrisService.GetAllReimbursementsAsync();
                return View(reimbursements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reimbursements");
                TempData["ErrorMessage"] = "Failed to load reimbursements.";
                return View(new List<HR_Management_System.Models.Reimbursement>());
            }
        }

        // GET: Expenses/MyExpenses
        [Authorize]
        public async Task<IActionResult> MyExpenses()
        {
            try
            {
                var employeeId = GetCurrentEmployeeId();
                var myTravelRequests = await _hrisService.GetAllTravelRequestsAsync(employeeId);
                var myReimbursements = await _hrisService.GetAllReimbursementsAsync(employeeId);
                
                ViewBag.MyTravelRequests = myTravelRequests;
                ViewBag.MyReimbursements = myReimbursements;
                
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user expenses");
                TempData["ErrorMessage"] = "Failed to load your expense data.";
                return View();
            }
        }

        // GET: Expenses/CreateTravel
        [Authorize]
        public IActionResult CreateTravel()
        {
            return View();
        }

        // GET: Expenses/CreateReimbursement
        [Authorize]
        public IActionResult CreateReimbursement()
        {
            return View();
        }

        private int GetCurrentEmployeeId()
        {
            // In a real application, you would get this from the current user
            // For now, return a default or get from claims
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            return 0; // Default or handle error
        }
    }
}