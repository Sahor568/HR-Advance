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
        private readonly IDepartmentService _departmentService;
        private readonly ITeamService _teamService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IEmployeeService employeeService,
            IAttendanceService attendanceService,
            ILeaveService leaveService,
            IPayrollService payrollService,
            IComplianceService complianceService,
            IDepartmentService departmentService,
            ITeamService teamService,
            ILogger<DashboardController> logger)
        {
            _employeeService = employeeService;
            _attendanceService = attendanceService;
            _leaveService = leaveService;
            _payrollService = payrollService;
            _complianceService = complianceService;
            _departmentService = departmentService;
            _teamService = teamService;
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
                
                var todayAttendances = await _attendanceService.GetTodayAttendancesAsync();
                var departments = await _departmentService.GetAllDepartmentsAsync();
                var teams = await _teamService.GetAllTeamsAsync();

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
                    TodayAttendance = todayAttendances.Count(),
                    PendingPayrolls = pendingPayrolls,
                    OpenAccidentLogs = openAccidents,
                    PendingInsuranceClaims = pendingInsuranceClaims,
                    UnresolvedDisciplinaryCases = unresolvedDisciplinaryCases,
                    TotalDepartments = departments.Count(),
                    TotalTeams = teams.Count(),
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
                var todayAttendances = await _attendanceService.GetTodayAttendancesAsync();
                var departments = await _departmentService.GetAllDepartmentsAsync();
                var teams = await _teamService.GetAllTeamsAsync();

                var currentMonth = DateTime.UtcNow.Month;
                var currentYear = DateTime.UtcNow.Year;
                var totalMonthlyPayroll = await _payrollService.GetTotalMonthlyPayrollAsync(currentMonth, currentYear);

                return Ok(new
                {
                    TotalEmployees = totalEmployees,
                    EmployeesOnProbation = employeesOnProbation,
                    PendingLeaveRequests = pendingLeaveRequests,
                    PendingPayrolls = pendingPayrolls,
                    OpenAccidents = openAccidents,
                    TodayAttendance = todayAttendances.Count(),
                    TotalDepartments = departments.Count(),
                    TotalTeams = teams.Count(),
                    TotalMonthlyPayroll = totalMonthlyPayroll
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard statistics");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("attendance-stats")]
        [Authorize(Roles = "Admin,HRManager")]
        public async Task<IActionResult> GetAttendanceStats()
        {
            try
            {
                var stats = await _attendanceService.GetAttendanceStatsLast7DaysAsync();
                
                // Convert to format expected by frontend
                var dates = stats.Keys.OrderBy(d => d).ToList();
                var presentData = dates.Select(d => stats[d].Present).ToList();
                var absentData = dates.Select(d => stats[d].Absent).ToList();
                var dayLabels = dates.Select(d => d.ToString("ddd")).ToList();
                
                return Ok(new
                {
                    labels = dayLabels,
                    present = presentData,
                    absent = absentData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching attendance statistics");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("leave-stats")]
        [Authorize(Roles = "Admin,HRManager")]
        public async Task<IActionResult> GetLeaveStats()
        {
            try
            {
                var stats = await _leaveService.GetLeaveStatsByStatusAsync();
                
                // Ensure we have all three statuses
                var approved = stats.ContainsKey("Approved") ? stats["Approved"] : 0;
                var pending = stats.ContainsKey("Pending") ? stats["Pending"] : 0;
                var rejected = stats.ContainsKey("Rejected") ? stats["Rejected"] : 0;
                
                return Ok(new
                {
                    approved,
                    pending,
                    rejected
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching leave statistics");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}