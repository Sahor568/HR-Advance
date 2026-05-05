using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HR_Management_System.Models.ViewModels;
using HR_Management_System.Services.Interfaces;

namespace HR_Management_System.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IAttendanceService _attendanceService;
        private readonly ILeaveService _leaveService;
        private readonly IPayrollService _payrollService;
        private readonly IComplianceService _complianceService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IEmployeeService employeeService,
            IAttendanceService attendanceService,
            ILeaveService leaveService,
            IPayrollService payrollService,
            IComplianceService complianceService,
            ILogger<DashboardController> logger)
        {
            _employeeService = employeeService;
            _attendanceService = attendanceService;
            _leaveService = leaveService;
            _payrollService = payrollService;
            _complianceService = complianceService;
            _logger = logger;
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin,HRManager")]
        public async Task<IActionResult> GetAdminDashboard()
        {
            try
            {
                var totalEmployees = await _employeeService.GetTotalActiveEmployeesCountAsync();
                var employeesOnProbation = await _employeeService.GetEmployeesOnProbationCountAsync();
                var pendingLeaveRequests = await _leaveService.GetPendingLeaveCountAsync();
                var pendingPayrolls = await _payrollService.GetPendingPayrollCountAsync();
                var openAccidents = await _complianceService.GetOpenAccidentCountAsync();
                var pendingInsuranceClaims = await _complianceService.GetPendingInsuranceClaimCountAsync();
                var unresolvedDisciplinaryCases = await _complianceService.GetUnresolvedDisciplinaryCountAsync();

                var currentMonth = DateTime.UtcNow.Month;
                var currentYear = DateTime.UtcNow.Year;
                var totalMonthlyPayroll = await _payrollService.GetTotalMonthlyPayrollAsync(currentMonth, currentYear);

                var upcomingProbationExpiries = await _employeeService.GetUpcomingProbationExpiriesAsync(30);

                var dashboard = new DashboardViewModel
                {
                    TotalEmployees = totalEmployees,
                    ActiveEmployees = totalEmployees,
                    EmployeesOnProbation = employeesOnProbation,
                    PendingLeaveRequests = pendingLeaveRequests,
                    TodayAttendance = 0, // Would need to calculate from attendance service
                    PendingPayrolls = pendingPayrolls,
                    OpenAccidentLogs = openAccidents,
                    PendingInsuranceClaims = pendingInsuranceClaims,
                    UnresolvedDisciplinaryCases = unresolvedDisciplinaryCases,
                    TotalMonthlyPayroll = totalMonthlyPayroll,
                    UpcomingProbationExpiries = upcomingProbationExpiries.ToList()
                };

                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching admin dashboard data");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("employee/{employeeId}")]
        [Authorize(Roles = "Admin,HRManager,Employee")]
        public async Task<IActionResult> GetEmployeeDashboard(int employeeId)
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
                if (employee == null)
                    return NotFound($"Employee with ID {employeeId} not found");

                var todayAttendance = await _attendanceService.GetTodayAttendanceAsync(employeeId);
                var leaveBalance = await _leaveService.GetLeaveBalanceAsync(employeeId, DateTime.UtcNow.Year);
                var upcomingProbationExpiries = await _employeeService.GetUpcomingProbationExpiriesAsync(30);

                return Ok(new
                {
                    Employee = employee.Employee,
                    TodayAttendance = todayAttendance,
                    LeaveBalance = leaveBalance,
                    UpcomingProbationExpiries = upcomingProbationExpiries.Where(e => e.EmployeeId == employeeId)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching employee dashboard for ID: {EmployeeId}", employeeId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("stats")]
        [Authorize(Roles = "Admin,HRManager")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var totalEmployees = await _employeeService.GetTotalActiveEmployeesCountAsync();
                var employeesOnProbation = await _employeeService.GetEmployeesOnProbationCountAsync();
                var pendingLeaveRequests = await _leaveService.GetPendingLeaveCountAsync();
                var pendingPayrolls = await _payrollService.GetPendingPayrollCountAsync();
                var openAccidents = await _complianceService.GetOpenAccidentCountAsync();

                return Ok(new
                {
                    TotalEmployees = totalEmployees,
                    EmployeesOnProbation = employeesOnProbation,
                    PendingLeaveRequests = pendingLeaveRequests,
                    PendingPayrolls = pendingPayrolls,
                    OpenAccidents = openAccidents
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard statistics");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}