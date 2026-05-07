using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using HR_Management_System.Data;
using HR_Management_System.Models;
using HR_Management_System.Models.ViewModels;
using HR_Management_System.Services.Interfaces;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HR_Management_System.Controllers
{
    [Authorize(Policy = "AdminOrHR")]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHRISService _hrisService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(ApplicationDbContext context, IHRISService hrisService, ILogger<NotificationsController> logger)
        {
            _context = context;
            _hrisService = hrisService;
            _logger = logger;
        }

        // GET: Notifications/Send
        public async Task<IActionResult> Send()
        {
            var model = new SendNotificationViewModel
            {
                AvailableDepartments = await GetDepartmentsSelectListAsync(),
                AvailableEmployees = await GetEmployeesSelectListAsync()
            };
            return View(model);
        }

        // POST: Notifications/Send
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(SendNotificationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableDepartments = await GetDepartmentsSelectListAsync();
                model.AvailableEmployees = await GetEmployeesSelectListAsync();
                return View(model);
            }

            try
            {
                List<string> userIds = new List<string>();

                if (model.SendToAllEmployees)
                {
                    // Get all active employees' user IDs from ApplicationUsers
                    var allUserIds = await _context.Users
                        .Where(u => u.EmployeeId.HasValue && u.IsActive)
                        .Select(u => u.Id)
                        .ToListAsync();
                    userIds.AddRange(allUserIds);
                }
                else if (model.DepartmentId.HasValue)
                {
                    // Get employees in selected department, then get their user IDs
                    var employeeIds = await _context.Employees
                        .Where(e => e.IsActive && e.DepartmentId == model.DepartmentId.Value)
                        .Select(e => e.Id)
                        .ToListAsync();
                    
                    var deptUserIds = await _context.Users
                        .Where(u => u.EmployeeId.HasValue && employeeIds.Contains(u.EmployeeId.Value))
                        .Select(u => u.Id)
                        .ToListAsync();
                    userIds.AddRange(deptUserIds);
                }
                else if (model.SelectedEmployeeIds != null && model.SelectedEmployeeIds.Any())
                {
                    // Get specific employees' user IDs
                    var empUserIds = await _context.Users
                        .Where(u => u.EmployeeId.HasValue && model.SelectedEmployeeIds.Contains(u.EmployeeId.Value))
                        .Select(u => u.Id)
                        .ToListAsync();
                    userIds.AddRange(empUserIds);
                }

                if (!userIds.Any())
                {
                    TempData["ErrorMessage"] = "No recipients found for the selected criteria.";
                    model.AvailableDepartments = await GetDepartmentsSelectListAsync();
                    model.AvailableEmployees = await GetEmployeesSelectListAsync();
                    return View(model);
                }

                // Send bulk notification
                var success = await _hrisService.SendBulkNotificationAsync(
                    userIds.Distinct().ToList(),
                    model.Title,
                    model.Message,
                    model.NotificationType.ToString()
                );

                if (success)
                {
                    TempData["SuccessMessage"] = $"Notification sent successfully to {userIds.Count} recipient(s).";
                    return RedirectToAction(nameof(Send));
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to send notification. Please try again.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk notification");
                TempData["ErrorMessage"] = $"An error occurred while sending notification: {ex.Message}";
            }

            model.AvailableDepartments = await GetDepartmentsSelectListAsync();
            model.AvailableEmployees = await GetEmployeesSelectListAsync();
            return View(model);
        }

        private async Task<List<SelectListItem>> GetDepartmentsSelectListAsync()
        {
            return await _context.Departments
                .Where(d => d.IsActive)
                .OrderBy(d => d.Name)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name
                })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetEmployeesSelectListAsync()
        {
            return await _context.Employees
                .Where(e => e.IsActive)
                .OrderBy(e => e.FullName)
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = $"{e.FullName} ({e.Emp_ID})"
                })
                .ToListAsync();
        }
    }

    public class SendNotificationViewModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Notification Type")]
        public NotificationType NotificationType { get; set; } = NotificationType.Info;

        [Display(Name = "Send to all employees")]
        public bool SendToAllEmployees { get; set; } = false;

        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }

        [Display(Name = "Specific Employees")]
        public List<int>? SelectedEmployeeIds { get; set; }

        // For dropdowns
        public List<SelectListItem>? AvailableDepartments { get; set; }
        public List<SelectListItem>? AvailableEmployees { get; set; }
    }
}