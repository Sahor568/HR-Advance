using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HR_Management_System.Models.ViewModels;
using HR_Management_System.Services.Interfaces;

namespace HR_Management_System.Controllers
{
    [Authorize(Roles = "Admin,HRManager")]
    [Route("api/[controller]")]
    [ApiController]
    public class ComplianceController : ControllerBase
    {
        private readonly IComplianceService _complianceService;
        private readonly ILogger<ComplianceController> _logger;

        public ComplianceController(IComplianceService complianceService, ILogger<ComplianceController> logger)
        {
            _complianceService = complianceService;
            _logger = logger;
        }

        [HttpPost("audit-report")]
        public async Task<IActionResult> GenerateLaborAuditReport([FromBody] AuditReportGenerateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

                var report = await _complianceService.GenerateLaborAuditReportAsync(model, userId);
                return Ok(new
                {
                    message = "Labor audit report generated successfully",
                    reportNumber = report.ReportNumber,
                    reportId = report.Id,
                    generatedDate = report.GeneratedDate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating labor audit report");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("accident/log")]
        public async Task<IActionResult> LogAccident([FromBody] AccidentLogCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var accidentLog = await _complianceService.LogAccidentAsync(model);
                return Ok(new
                {
                    message = "Accident logged successfully",
                    accidentId = accidentLog.Id,
                    severity = accidentLog.Severity.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging accident");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("accident")]
        public async Task<IActionResult> GetAccidentLogs([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            try
            {
                var logs = await _complianceService.GetAccidentLogsAsync(from, to);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching accident logs");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("insurance-claim")]
        [Authorize(Roles = "Admin,HRManager,Employee")]
        public async Task<IActionResult> SubmitInsuranceClaim([FromBody] MedicalClaimCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var claim = await _complianceService.SubmitInsuranceClaimAsync(model);
                return Ok(new
                {
                    message = "Insurance claim submitted successfully",
                    claimNumber = claim.ClaimNumber,
                    claimId = claim.Id,
                    status = claim.Status.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting insurance claim");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("insurance-claim/approve/{id}")]
        public async Task<IActionResult> ApproveInsuranceClaim(int id, [FromQuery] decimal approvedAmount, [FromQuery] string remarks)
        {
            try
            {
                if (string.IsNullOrEmpty(remarks))
                    return BadRequest("Approval remarks are required");

                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

                var result = await _complianceService.ApproveInsuranceClaimAsync(id, userId, approvedAmount, remarks);
                if (!result)
                    return BadRequest("Unable to approve insurance claim");

                return Ok(new { message = "Insurance claim approved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving insurance claim ID: {ClaimId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("insurance-claim/reject/{id}")]
        public async Task<IActionResult> RejectInsuranceClaim(int id, [FromQuery] string reason)
        {
            try
            {
                if (string.IsNullOrEmpty(reason))
                    return BadRequest("Rejection reason is required");

                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

                var result = await _complianceService.RejectInsuranceClaimAsync(id, userId, reason);
                if (!result)
                    return NotFound("Insurance claim not found");

                return Ok(new { message = "Insurance claim rejected" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting insurance claim ID: {ClaimId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("insurance-claim/pending")]
        public async Task<IActionResult> GetPendingInsuranceClaims()
        {
            try
            {
                var claims = await _complianceService.GetPendingClaimsAsync();
                return Ok(claims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pending insurance claims");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("disciplinary")]
        public async Task<IActionResult> CreateDisciplinaryRecord([FromBody] DisciplinaryCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

                var record = await _complianceService.CreateDisciplinaryRecordAsync(model, userId);
                return Ok(new
                {
                    message = "Disciplinary record created successfully",
                    recordId = record.Id,
                    actionType = record.ActionType.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating disciplinary record");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("disciplinary/resolve/{id}")]
        public async Task<IActionResult> ResolveDisciplinaryCase(int id, [FromQuery] string resolution)
        {
            try
            {
                if (string.IsNullOrEmpty(resolution))
                    return BadRequest("Resolution details are required");

                var result = await _complianceService.ResolveDisciplinaryCaseAsync(id, resolution);
                if (!result)
                    return NotFound("Disciplinary record not found");

                return Ok(new { message = "Disciplinary case resolved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving disciplinary case ID: {RecordId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("disciplinary/unresolved")]
        public async Task<IActionResult> GetUnresolvedDisciplinaryCases()
        {
            try
            {
                var cases = await _complianceService.GetUnresolvedDisciplinaryCasesAsync();
                return Ok(cases);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching unresolved disciplinary cases");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetComplianceStats()
        {
            try
            {
                var openAccidents = await _complianceService.GetOpenAccidentCountAsync();
                var pendingClaims = await _complianceService.GetPendingInsuranceClaimCountAsync();
                var unresolvedDisciplinary = await _complianceService.GetUnresolvedDisciplinaryCountAsync();

                return Ok(new
                {
                    OpenAccidents = openAccidents,
                    PendingInsuranceClaims = pendingClaims,
                    UnresolvedDisciplinaryCases = unresolvedDisciplinary
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching compliance statistics");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}