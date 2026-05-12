using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HR_Management_System.Models.ViewModels;
using HR_Management_System.Services.Interfaces;
using System.Security.Claims;

namespace HR_Management_System.Controllers
{
    [Authorize]
    [Route("Attendance")]
    public class AttendanceMvcController : Controller
    {
        private readonly IAttendanceService _attendanceService;
        private readonly ILogger<AttendanceMvcController> _logger;

        public AttendanceMvcController(IAttendanceService attendanceService, ILogger<AttendanceMvcController> logger)
        {
            _attendanceService = attendanceService;
            _logger = logger;
        }

        // GET: Attendance
        public IActionResult Index()
        {
            return View();
        }

        // GET: Attendance/Pending
        [Authorize(Roles = "Admin,HRManager")]
        public async Task<IActionResult> Pending([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var pendingAttendances = await _attendanceService.GetPendingAttendancesAsync(fromDate, toDate);
                return View(pendingAttendances);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pending attendances");
                TempData["ErrorMessage"] = "An error occurred while fetching pending attendance requests.";
                return View(new List<AttendanceListViewModel>());
            }
        }

        // POST: Attendance/Approve/{id}
        [HttpPost]
        [Authorize(Roles = "Admin,HRManager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, [FromForm] string remarks)
        {
            try
            {
                var approvedBy = User.FindFirstValue(ClaimTypes.Name) ?? "System";
                var result = await _attendanceService.ApproveAttendanceAsync(id, approvedBy, remarks);

                if (result)
                {
                    TempData["SuccessMessage"] = "Attendance approved successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to approve attendance. It may have been already processed.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving attendance {AttendanceId}", id);
                TempData["ErrorMessage"] = "An error occurred while approving attendance.";
            }

            return RedirectToAction(nameof(Pending));
        }

        // POST: Attendance/Reject/{id}
        [HttpPost]
        [Authorize(Roles = "Admin,HRManager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, [FromForm] string remarks)
        {
            try
            {
                var rejectedBy = User.FindFirstValue(ClaimTypes.Name) ?? "System";
                var result = await _attendanceService.RejectAttendanceAsync(id, rejectedBy, remarks);

                if (result)
                {
                    TempData["SuccessMessage"] = "Attendance rejected successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to reject attendance. It may have been already processed.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting attendance {AttendanceId}", id);
                TempData["ErrorMessage"] = "An error occurred while rejecting attendance.";
            }

            return RedirectToAction(nameof(Pending));
        }
    }
}