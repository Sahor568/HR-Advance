using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HR_Management_System.Models.ViewModels;
using HR_Management_System.Services.Interfaces;

namespace HR_Management_System.Controllers
{
    [Authorize(Roles = "Admin,HRManager")]
    [Route("api/[controller]")]
    [ApiController]
    public class PayrollController : ControllerBase
    {
        private readonly IPayrollService _payrollService;
        private readonly ILogger<PayrollController> _logger;

        public PayrollController(IPayrollService payrollService, ILogger<PayrollController> logger)
        {
            _payrollService = payrollService;
            _logger = logger;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GeneratePayroll([FromBody] PayrollGenerateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (model.EmployeeId.HasValue)
                {
                    var payroll = await _payrollService.GeneratePayrollAsync(
                        model.EmployeeId.Value,
                        model.Month,
                        model.Year,
                        model.IncludeFestivalAllowance
                    );
                    return Ok(new
                    {
                        message = "Payroll generated successfully",
                        payrollId = payroll.Id,
                        netPay = payroll.Net_Pay
                    });
                }
                else
                {
                    var payrolls = await _payrollService.GenerateBulkPayrollAsync(
                        model.Month,
                        model.Year,
                        model.IncludeFestivalAllowance
                    );
                    return Ok(new
                    {
                        message = "Bulk payroll generation completed",
                        count = payrolls.Count(),
                        totalNetPay = payrolls.Sum(p => p.Net_Pay)
                    });
                }
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Employee not found for payroll generation");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating payroll");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("approve")]
        public async Task<IActionResult> ApprovePayroll([FromBody] PayrollApprovalViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

                var result = await _payrollService.ApprovePayrollAsync(model.PayrollId, userId);
                if (!result)
                    return NotFound("Payroll not found");

                return Ok(new { message = "Payroll approved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving payroll ID: {PayrollId}", model.PayrollId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("mark-paid/{id}")]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            try
            {
                var result = await _payrollService.MarkAsPaidAsync(id);
                if (!result)
                    return BadRequest("Payroll must be approved before marking as paid");

                return Ok(new { message = "Payroll marked as paid" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking payroll ID: {PayrollId} as paid", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("payslip/{id}")]
        [Authorize(Roles = "Admin,HRManager,Employee")]
        public async Task<IActionResult> GetPayslip(int id)
        {
            try
            {
                var payslip = await _payrollService.GetPayslipAsync(id);
                return Ok(payslip);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Payslip not found for ID: {PayrollId}", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching payslip for payroll ID: {PayrollId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("employee/{employeeId}")]
        [Authorize(Roles = "Admin,HRManager,Employee")]
        public async Task<IActionResult> GetEmployeePayslips(int employeeId)
        {
            try
            {
                var payslips = await _payrollService.GetEmployeePayslipsAsync(employeeId);
                return Ok(payslips);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching payslips for employee ID: {EmployeeId}", employeeId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("monthly/{month}/{year}")]
        public async Task<IActionResult> GetMonthlyPayrolls(int month, int year)
        {
            try
            {
                var payrolls = await _payrollService.GetPayrollsByMonthYearAsync(month, year);
                return Ok(payrolls);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching payrolls for {Month}/{Year}", month, year);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("calculate/tds")]
        public async Task<IActionResult> CalculateTDS([FromQuery] decimal annualTaxableIncome)
        {
            try
            {
                var tds = await _payrollService.CalculateTDSCalculationAsync(annualTaxableIncome);
                return Ok(new
                {
                    annualTaxableIncome,
                    monthlyTDS = tds,
                    annualTDS = tds * 12
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating TDS for income: {Income}", annualTaxableIncome);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("calculate/overtime")]
        public async Task<IActionResult> CalculateOvertimePay([FromQuery] decimal baseSalary, [FromQuery] decimal overtimeHours)
        {
            try
            {
                var overtimePay = await _payrollService.CalculateOvertimePayAsync(baseSalary, overtimeHours);
                return Ok(new
                {
                    baseSalary,
                    overtimeHours,
                    overtimePay,
                    hourlyRate = baseSalary / 208,
                    overtimeRate = (baseSalary / 208) * 1.5m
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating overtime pay");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("stats/pending-count")]
        public async Task<IActionResult> GetPendingPayrollCount()
        {
            try
            {
                var count = await _payrollService.GetPendingPayrollCountAsync();
                return Ok(new { pendingPayrollCount = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pending payroll count");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("monthly-total")]
        public async Task<IActionResult> GetCurrentMonthlyPayrollTotal()
        {
            try
            {
                var now = DateTime.UtcNow;
                var month = now.Month;
                var year = now.Year;
                
                var total = await _payrollService.GetTotalMonthlyPayrollAsync(month, year);
                return Ok(new
                {
                    month,
                    year,
                    totalPayrollAmount = total
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching current monthly payroll total");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("stats/monthly-total/{month}/{year}")]
        public async Task<IActionResult> GetMonthlyPayrollTotal(int month, int year)
        {
            try
            {
                var total = await _payrollService.GetTotalMonthlyPayrollAsync(month, year);
                return Ok(new
                {
                    month,
                    year,
                    totalPayrollAmount = total
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching monthly payroll total for {Month}/{Year}", month, year);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("ssf/generate")]
        public async Task<IActionResult> GenerateSSFRecord([FromQuery] int employeeId, [FromQuery] int month, [FromQuery] int year)
        {
            try
            {
                var ssfRecord = await _payrollService.GenerateSSFRecordAsync(employeeId, month, year);
                return Ok(new
                {
                    message = "SSF record generated successfully",
                    recordId = ssfRecord.Id,
                    employeeContribution = ssfRecord.EmployeeContribution,
                    employerContribution = ssfRecord.EmployerContribution,
                    totalBalance = ssfRecord.TotalBalance
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Employee not found for SSF record generation");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating SSF record");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}