using HR_Management_System.Models;
using HR_Management_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace HR_Management_System.Controllers
{
    [Authorize(Policy = "EmployeeOnly")]
    public class EmployeeSelfServiceController : Controller
    {
        private readonly IEmployeeSelfService _employeeSelfService;
        private readonly IEmployeeService _employeeService;
        private readonly ILeaveService _leaveService;
        private readonly IPayrollService _payrollService;
        private readonly ILogger<EmployeeSelfServiceController> _logger;

        public EmployeeSelfServiceController(
            IEmployeeSelfService employeeSelfService,
            IEmployeeService employeeService,
            ILeaveService leaveService,
            IPayrollService payrollService,
            ILogger<EmployeeSelfServiceController> logger)
        {
            _employeeSelfService = employeeSelfService;
            _employeeService = employeeService;
            _leaveService = leaveService;
            _payrollService = payrollService;
            _logger = logger;
        }

        // GET: EmployeeSelfService/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByUserIdAsync(User.Identity.Name);
                if (employee == null)
                {
                    return NotFound();
                }

                var dashboardData = await _employeeSelfService.GetEmployeeDashboardAsync(employee.Id);
                return View(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading employee dashboard");
                TempData["ErrorMessage"] = "Failed to load dashboard.";
                return View();
            }
        }

        // GET: EmployeeSelfService/Profile
        public async Task<IActionResult> Profile()
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByUserIdAsync(User.Identity.Name);
                if (employee == null)
                {
                    return NotFound();
                }
                return View(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading employee profile");
                TempData["ErrorMessage"] = "Failed to load profile.";
                return RedirectToAction(nameof(Dashboard));
            }
        }

        // GET: EmployeeSelfService/EditProfile
        public async Task<IActionResult> EditProfile()
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByUserIdAsync(User.Identity.Name);
                if (employee == null)
                {
                    return NotFound();
                }
                return View(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading employee profile for edit");
                TempData["ErrorMessage"] = "Failed to load profile for editing.";
                return RedirectToAction(nameof(Profile));
            }
        }

        // POST: EmployeeSelfService/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(Employee employee)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var currentEmployee = await _employeeService.GetEmployeeByUserIdAsync(User.Identity.Name);
                    if (currentEmployee == null || currentEmployee.Id != employee.Id)
                    {
                        return NotFound();
                    }

                    // Only allow updating certain fields in self-service
                    currentEmployee.PhoneNumber = employee.PhoneNumber;
                    currentEmployee.AlternatePhone = employee.AlternatePhone;
                    currentEmployee.Email = employee.Email;
                    currentEmployee.TemporaryAddress = employee.TemporaryAddress;
                    currentEmployee.EmergencyContactName = employee.EmergencyContactName;
                    currentEmployee.EmergencyContactPhone = employee.EmergencyContactPhone;
                    currentEmployee.EmergencyContactRelationship = employee.EmergencyContactRelationship;

                    await _employeeService.UpdateEmployeeAsync(currentEmployee.Id, currentEmployee);
                    _logger.LogInformation("Employee {EmployeeId} updated their profile", employee.Id);
                    
                    TempData["SuccessMessage"] = "Profile updated successfully.";
                    return RedirectToAction(nameof(Profile));
                }
                return View(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating employee profile");
                TempData["ErrorMessage"] = "Failed to update profile.";
                return View(employee);
            }
        }

        // GET: EmployeeSelfService/Payslips
        public async Task<IActionResult> Payslips(int? year, int? month)
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByUserIdAsync(User.Identity.Name);
                if (employee == null)
                {
                    return NotFound();
                }

                var payslips = await _payrollService.GetEmployeePayslipsAsync(employee.Id, year, month);
                return View(payslips);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading employee payslips");
                TempData["ErrorMessage"] = "Failed to load payslips.";
                return View(new List<Payroll>());
            }
        }

        // GET: EmployeeSelfService/PayslipDetails/5
        public async Task<IActionResult> PayslipDetails(int id)
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByUserIdAsync(User.Identity.Name);
                if (employee == null)
                {
                    return NotFound();
                }

                var payslip = await _payrollService.GetPayslipByIdAsync(id);
                if (payslip == null || payslip.EmployeeId != employee.Id)
                {
                    return NotFound();
                }

                return View(payslip);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading payslip details with ID {id}");
                TempData["ErrorMessage"] = "Failed to load payslip details.";
                return RedirectToAction(nameof(Payslips));
            }
        }

        // GET: EmployeeSelfService/DownloadPayslip/5
        public async Task<IActionResult> DownloadPayslip(int id)
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByUserIdAsync(User.Identity.Name);
                if (employee == null)
                {
                    return NotFound();
                }

                var payslip = await _payrollService.GetPayslipByIdAsync(id);
                if (payslip == null || payslip.EmployeeId != employee.Id)
                {
                    return NotFound();
                }

                var pdfBytes = await _payrollService.GeneratePayslipPdfAsync(id);
                return File(pdfBytes, "application/pdf", $"Payslip-{payslip.Month}-{payslip.Year}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading payslip with ID {id}");
                TempData["ErrorMessage"] = "Failed to download payslip.";
                return RedirectToAction(nameof(Payslips));
            }
        }

        // GET: EmployeeSelfService/LeaveBalance
        public async Task<IActionResult> LeaveBalance()
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByUserIdAsync(User.Identity.Name);
                if (employee == null)
                {
                    return NotFound();
                }

                var currentYear = DateTime.Now.Year;
                var leaveBalance = await _leaveService.GetLeaveBalanceAsync(employee.Id, currentYear);
                return View(leaveBalance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading leave balance");
                TempData["ErrorMessage"] = "Failed to load leave balance.";
                return View(new LeaveBalance());
            }
        }

        // GET: EmployeeSelfService/ApplyLeave
        public async Task<IActionResult> ApplyLeave()
        {
            return View();
        }

        // POST: EmployeeSelfService/ApplyLeave
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyLeave(LeaveRequest leaveRequest)
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByUserIdAsync(User.Identity.Name);
                if (employee == null)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    leaveRequest.EmployeeId = employee.Id;
                    leaveRequest.Status = LeaveStatus.Pending;
                    leaveRequest.AppliedDate = DateTime.Now;

                    await _leaveService.CreateLeaveRequestAsync(leaveRequest);
                    _logger.LogInformation("Employee {EmployeeId} applied for leave from {StartDate} to {EndDate}", 
                        employee.Id, leaveRequest.StartDate, leaveRequest.EndDate);
                    
                    TempData["SuccessMessage"] = "Leave application submitted successfully.";
                    return RedirectToAction(nameof(MyLeaveApplications));
                }
                return View(leaveRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying for leave");
                TempData["ErrorMessage"] = "Failed to submit leave application.";
                return View(leaveRequest);
            }
        }

        // GET: EmployeeSelfService/MyLeaveApplications
        public async Task<IActionResult> MyLeaveApplications()
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByUserIdAsync(User.Identity.Name);
                if (employee == null)
                {
                    return NotFound();
                }

                var leaveRequests = await _leaveService.GetLeaveRequestsByEmployeeIdAsync(employee.Id);
                return View(leaveRequests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading leave applications");
                TempData["ErrorMessage"] = "Failed to load leave applications.";
                return View(new List<LeaveRequest>());
            }
        }

        // GET: EmployeeSelfService/CancelLeave/5
        public async Task<IActionResult> CancelLeave(int id)
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByUserIdAsync(User.Identity.Name);
                if (employee == null)
                {
                    return NotFound();
                }

                var leaveRequest = await _leaveService.GetLeaveRequestByIdAsync(id);
                if (leaveRequest == null || leaveRequest.EmployeeId != employee.Id)
                {
                    return NotFound();
                }

                if (leaveRequest.Status != LeaveStatus.Pending)
                {
                    TempData["ErrorMessage"] = "Only pending leave requests can be cancelled.";
                    return RedirectToAction(nameof(MyLeaveApplications));
                }

                return View(leaveRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading leave request for cancellation with ID {id}");
                TempData["ErrorMessage"] = "Failed to load leave request for cancellation.";
                return RedirectToAction(nameof(MyLeaveApplications));
            }
        }

        // POST: EmployeeSelfService/CancelLeave/5
        [HttpPost, ActionName("CancelLeave")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelLeaveConfirmed(int id)
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByUserIdAsync(User.Identity.Name);
                if (employee == null)
                {
                    return NotFound();
                }

                var leaveRequest = await _leaveService.GetLeaveRequestByIdAsync(id);
                if (leaveRequest == null || leaveRequest.EmployeeId != employee.Id)
                {
                    return NotFound();
                }

                await _leaveService.CancelLeaveRequestAsync(id);
                _logger.LogInformation("Employee {EmployeeId} cancelled leave request {LeaveRequestId}", employee.Id, id);
                
                TempData["SuccessMessage"] = "Leave request cancelled successfully.";
                return RedirectToAction(nameof(MyLeaveApplications));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error cancelling leave request with ID {id}");
                TempData["ErrorMessage"] = "Failed to cancel leave request.";
                return RedirectToAction(nameof(CancelLeave), new { id });
            }
        }

        // GET: EmployeeSelfService/Attendance
        public async Task<IActionResult> Attendance(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByUserIdAsync(User.Identity.Name);
                if (employee == null)
                {
                    return NotFound();
                }

                var attendance = await _employeeSelfService.GetEmployeeAttendanceAsync(
                    employee.Id, 
                    fromDate ?? DateTime.Now.AddDays(-30), 
                    toDate ?? DateTime.Now);
                
                return View(attendance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading attendance records");
                TempData["ErrorMessage"] = "Failed to load attendance records.";
                return View(new List<Attendance>());
            }
        }

        // GET: EmployeeSelfService/SSFBalance
        public async Task<IActionResult> SSFBalance()
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByUserIdAsync(User.Identity.Name);
                if (employee == null)
                {
                    return NotFound();
                }

                var ssfBalance = await _employeeSelfService.GetSSFBalanceAsync(employee.Id);
                return View(ssfBalance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading SSF balance");
                TempData["ErrorMessage"] = "Failed to load SSF balance.";
                return View();
            }
        }

        // GET: EmployeeSelfService/TaxDocuments
        public async Task<IActionResult> TaxDocuments(int? year)
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByUserIdAsync(User.Identity.Name);
                if (employee == null)
                {
                    return NotFound();
                }

                var taxYear = year ?? DateTime.Now.Year;
                var taxDocuments = await _employeeSelfService.GetTaxDocumentsAsync(employee.Id, taxYear);
                return View(taxDocuments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tax documents");
                TempData["ErrorMessage"] = "Failed to load tax documents.";
                return View(new List<object>());
            }
        }

        // GET: EmployeeSelfService/DownloadTaxCertificate/2024
        public async Task<IActionResult> DownloadTaxCertificate(int year)
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByUserIdAsync(User.Identity.Name);
                if (employee == null)
                {
                    return NotFound();
                }

                var pdfBytes = await _employeeSelfService.GenerateTaxCertificateAsync(employee.Id, year);
                return File(pdfBytes, "application/pdf", $"Tax-Certificate-{year}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading tax certificate for year {year}");
                TempData["ErrorMessage"] = "Failed to download tax certificate.";
                return RedirectToAction(nameof(TaxDocuments));
            }
        }

        // GET: EmployeeSelfService/Notifications
        public async Task<IActionResult> Notifications()
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByUserIdAsync(User.Identity.Name);
                if (employee == null)
                {
                    return NotFound();
                }

                var notifications = await _employeeSelfService.GetNotificationsAsync(employee.Id);
                return View(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading notifications");
                TempData["ErrorMessage"] = "Failed to load notifications.";
                return View(new List<Notification>());
            }
        }

        // POST: EmployeeSelfService/MarkNotificationAsRead/5
        [HttpPost]
        public async Task<IActionResult> MarkNotificationAsRead(int id)
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByUserIdAsync(User.Identity.Name);
                if (employee == null)
                {
                    return Json(new { success = false, message = "Employee not found" });
                }

                await _employeeSelfService.MarkNotificationAsReadAsync(id, employee.Id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking notification {id} as read");
                return Json(new { success = false, message = "Failed to mark notification as read" });
            }
        }

        // GET: EmployeeSelfService/ChangePassword
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: EmployeeSelfService/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            try
            {
                if (newPassword != confirmPassword)
                {
                    ModelState.AddModelError("confirmPassword", "New password and confirmation password do not match.");
                    return View();
                }

                var result = await _employeeSelfService.ChangePasswordAsync(User.Identity.Name, currentPassword, newPassword);
                if (result)
                {
                    _logger.LogInformation("Employee changed password successfully");
                    TempData["SuccessMessage"] = "Password changed successfully.";
                    return RedirectToAction(nameof(Profile));
                }
                else
                {
                    ModelState.AddModelError("currentPassword", "Current password is incorrect.");
                    return View();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                TempData["ErrorMessage"] = "Failed to change password.";
                return View();
            }
        }
    }
}