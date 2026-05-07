using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HR_Management_System.Models.ViewModels;
using HR_Management_System.Services.Interfaces;

namespace HR_Management_System.Controllers
{
    [Authorize(Roles = "Admin,HRManager")]
    [Route("Compliance")]
    public class ComplianceMvcController : Controller
    {
        private readonly IComplianceService _complianceService;
        private readonly ILogger<ComplianceMvcController> _logger;

        public ComplianceMvcController(IComplianceService complianceService, ILogger<ComplianceMvcController> logger)
        {
            _complianceService = complianceService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("audit-reports")]
        public IActionResult AuditReports()
        {
            return View();
        }

        [HttpGet("audit-report/create")]
        public IActionResult CreateAuditReport()
        {
            return View();
        }

        [HttpPost("audit-report/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAuditReport(AuditReportGenerateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var report = await _complianceService.GenerateLaborAuditReportAsync(model, userId);
                
                TempData["SuccessMessage"] = $"Audit report {report.ReportNumber} generated successfully.";
                return RedirectToAction("AuditReports");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating labor audit report");
                ModelState.AddModelError("", "An error occurred while generating the audit report.");
                return View(model);
            }
        }

        [HttpGet("accidents")]
        public IActionResult Accidents()
        {
            return View();
        }

        [HttpGet("accident/create")]
        public IActionResult CreateAccidentLog()
        {
            return View();
        }

        [HttpPost("accident/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAccidentLog(AccidentLogCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var accidentLog = await _complianceService.LogAccidentAsync(model);
                
                TempData["SuccessMessage"] = $"Accident logged successfully (Severity: {accidentLog.Severity}).";
                return RedirectToAction("Accidents");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging accident");
                ModelState.AddModelError("", "An error occurred while logging the accident.");
                return View(model);
            }
        }

        [HttpGet("insurance-claims")]
        public IActionResult InsuranceClaims()
        {
            return View();
        }

        [HttpGet("insurance-claim/create")]
        [Authorize(Roles = "Admin,HRManager,Employee")]
        public IActionResult CreateInsuranceClaim()
        {
            return View();
        }

        [HttpPost("insurance-claim/create")]
        [Authorize(Roles = "Admin,HRManager,Employee")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateInsuranceClaim(MedicalClaimCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var claim = await _complianceService.SubmitInsuranceClaimAsync(model);
                
                TempData["SuccessMessage"] = $"Insurance claim {claim.ClaimNumber} submitted successfully.";
                return RedirectToAction("InsuranceClaims");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting insurance claim");
                ModelState.AddModelError("", "An error occurred while submitting the insurance claim.");
                return View(model);
            }
        }

        [HttpGet("disciplinary-cases")]
        public IActionResult DisciplinaryCases()
        {
            return View();
        }

        [HttpGet("disciplinary/create")]
        public IActionResult CreateDisciplinaryCase()
        {
            return View();
        }

        [HttpPost("disciplinary/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDisciplinaryCase(DisciplinaryCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var disciplinaryRecord = await _complianceService.CreateDisciplinaryRecordAsync(model, userId);
                
                TempData["SuccessMessage"] = $"Disciplinary case created successfully.";
                return RedirectToAction("DisciplinaryCases");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating disciplinary case");
                ModelState.AddModelError("", "An error occurred while creating the disciplinary case.");
                return View(model);
            }
        }

        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpGet("stats")]
        public async Task<IActionResult> Stats()
        {
            try
            {
                var openAccidents = await _complianceService.GetOpenAccidentCountAsync();
                var pendingClaims = await _complianceService.GetPendingInsuranceClaimCountAsync();
                var unresolvedDisciplinary = await _complianceService.GetUnresolvedDisciplinaryCountAsync();
                
                var stats = new
                {
                    OpenAccidents = openAccidents,
                    PendingClaims = pendingClaims,
                    UnresolvedDisciplinary = unresolvedDisciplinary,
                    TotalComplianceItems = openAccidents + pendingClaims + unresolvedDisciplinary
                };
                
                return View(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching compliance statistics");
                TempData["ErrorMessage"] = "An error occurred while fetching compliance statistics.";
                return RedirectToAction("Index");
            }
        }
    }
}