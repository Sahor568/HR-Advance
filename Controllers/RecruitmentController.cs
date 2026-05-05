using HR_Management_System.Models.Recruitment;
using HR_Management_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace HR_Management_System.Controllers
{
    [Authorize]
    public class RecruitmentController : Controller
    {
        private readonly IRecruitmentService _recruitmentService;
        private readonly ILogger<RecruitmentController> _logger;

        public RecruitmentController(
            IRecruitmentService recruitmentService,
            ILogger<RecruitmentController> logger)
        {
            _recruitmentService = recruitmentService;
            _logger = logger;
        }

        // GET: Recruitment/Index
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var positions = await _recruitmentService.GetAllJobPositionsAsync();
                return View(positions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving job positions");
                TempData["ErrorMessage"] = "Failed to load job positions.";
                return View(new List<JobPosition>());
            }
        }

        // GET: Recruitment/JobDetails/5
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> JobDetails(int id)
        {
            try
            {
                var position = await _recruitmentService.GetJobPositionByIdAsync(id);
                if (position == null)
                {
                    return NotFound();
                }
                return View(position);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving job position with ID {id}");
                TempData["ErrorMessage"] = "Failed to load job details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Recruitment/CreateJob
        [Authorize(Policy = "AdminOrHR")]
        public IActionResult CreateJob()
        {
            return View();
        }

        // POST: Recruitment/CreateJob
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> CreateJob(JobPosition position)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _recruitmentService.CreateJobPositionAsync(position);
                    _logger.LogInformation("Created job position: {PositionTitle} by user {User}", position.PositionTitle, User.Identity.Name);
                    
                    TempData["SuccessMessage"] = "Job position created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                return View(position);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating job position");
                TempData["ErrorMessage"] = "Failed to create job position.";
                return View(position);
            }
        }

        // GET: Recruitment/EditJob/5
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> EditJob(int id)
        {
            try
            {
                var position = await _recruitmentService.GetJobPositionByIdAsync(id);
                if (position == null)
                {
                    return NotFound();
                }
                return View(position);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving job position for edit with ID {id}");
                TempData["ErrorMessage"] = "Failed to load job position for editing.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Recruitment/EditJob/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> EditJob(int id, JobPosition position)
        {
            if (id != position.Id)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    await _recruitmentService.UpdateJobPositionAsync(id, position);
                    _logger.LogInformation("Updated job position {PositionId}: {PositionTitle} by user {User}", id, position.PositionTitle, User.Identity.Name);
                    
                    TempData["SuccessMessage"] = "Job position updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                return View(position);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating job position with ID {id}");
                TempData["ErrorMessage"] = "Failed to update job position.";
                return View(position);
            }
        }

        // GET: Recruitment/DeleteJob/5
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            try
            {
                var position = await _recruitmentService.GetJobPositionByIdAsync(id);
                if (position == null)
                {
                    return NotFound();
                }
                return View(position);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving job position for delete with ID {id}");
                TempData["ErrorMessage"] = "Failed to load job position for deletion.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Recruitment/DeleteJob/5
        [HttpPost, ActionName("DeleteJob")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteJobConfirmed(int id)
        {
            try
            {
                await _recruitmentService.DeleteJobPositionAsync(id);
                _logger.LogInformation("Deleted job position {PositionId} by user {User}", id, User.Identity.Name);
                
                TempData["SuccessMessage"] = "Job position deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting job position with ID {id}");
                TempData["ErrorMessage"] = "Failed to delete job position.";
                return RedirectToAction(nameof(DeleteJob), new { id });
            }
        }

        // GET: Recruitment/Candidates
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> Candidates()
        {
            try
            {
                var candidates = await _recruitmentService.GetAllCandidatesAsync();
                return View(candidates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving candidates");
                TempData["ErrorMessage"] = "Failed to load candidates.";
                return View(new List<Candidate>());
            }
        }

        // GET: Recruitment/CandidateDetails/5
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> CandidateDetails(int id)
        {
            try
            {
                var candidate = await _recruitmentService.GetCandidateByIdAsync(id);
                if (candidate == null)
                {
                    return NotFound();
                }
                return View(candidate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving candidate with ID {id}");
                TempData["ErrorMessage"] = "Failed to load candidate details.";
                return RedirectToAction(nameof(Candidates));
            }
        }

        // GET: Recruitment/CreateCandidate
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> CreateCandidate()
        {
            ViewBag.JobPositions = await _recruitmentService.GetAllJobPositionsAsync();
            return View();
        }

        // POST: Recruitment/CreateCandidate
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> CreateCandidate(Candidate candidate)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _recruitmentService.CreateCandidateAsync(candidate);
                    _logger.LogInformation("Created candidate for position {PositionId} by user {User}",
                        candidate.JobPositionId, User.Identity.Name);
                    
                    TempData["SuccessMessage"] = "Candidate created successfully.";
                    return RedirectToAction(nameof(Candidates));
                }
                ViewBag.JobPositions = await _recruitmentService.GetAllJobPositionsAsync();
                return View(candidate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating candidate");
                TempData["ErrorMessage"] = "Failed to create candidate.";
                ViewBag.JobPositions = await _recruitmentService.GetAllJobPositionsAsync();
                return View(candidate);
            }
        }

        // GET: Recruitment/Interviews
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> Interviews()
        {
            try
            {
                var interviews = await _recruitmentService.GetAllInterviewsAsync();
                return View(interviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving interviews");
                TempData["ErrorMessage"] = "Failed to load interviews.";
                return View(new List<Interview>());
            }
        }

        // GET: Recruitment/ScheduleInterview
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> ScheduleInterview()
        {
            ViewBag.Candidates = await _recruitmentService.GetAllCandidatesAsync();
            ViewBag.JobPositions = await _recruitmentService.GetAllJobPositionsAsync();
            return View();
        }

        // POST: Recruitment/ScheduleInterview
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> ScheduleInterview(Interview interview)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _recruitmentService.ScheduleInterviewAsync(interview);
                    _logger.LogInformation("Scheduled interview for candidate {CandidateId} by user {User}", 
                        interview.CandidateId, User.Identity.Name);
                    
                    TempData["SuccessMessage"] = "Interview scheduled successfully.";
                    return RedirectToAction(nameof(Interviews));
                }
                ViewBag.Candidates = await _recruitmentService.GetAllCandidatesAsync();
                ViewBag.JobPositions = await _recruitmentService.GetAllJobPositionsAsync();
                return View(interview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling interview");
                TempData["ErrorMessage"] = "Failed to schedule interview.";
                ViewBag.Candidates = await _recruitmentService.GetAllCandidatesAsync();
                ViewBag.JobPositions = await _recruitmentService.GetAllJobPositionsAsync();
                return View(interview);
            }
        }

        // GET: Recruitment/OfferLetters
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> OfferLetters()
        {
            try
            {
                var offerLetters = await _recruitmentService.GetAllOfferLettersAsync();
                return View(offerLetters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving offer letters");
                TempData["ErrorMessage"] = "Failed to load offer letters.";
                return View(new List<OfferLetter>());
            }
        }

        // GET: Recruitment/CreateOfferLetter
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> CreateOfferLetter()
        {
            ViewBag.Candidates = await _recruitmentService.GetAllCandidatesAsync();
            ViewBag.JobPositions = await _recruitmentService.GetAllJobPositionsAsync();
            return View();
        }

        // POST: Recruitment/CreateOfferLetter
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> CreateOfferLetter(OfferLetter offerLetter)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _recruitmentService.CreateOfferLetterAsync(offerLetter);
                    _logger.LogInformation("Created offer letter for candidate {CandidateId} by user {User}", 
                        offerLetter.CandidateId, User.Identity.Name);
                    
                    TempData["SuccessMessage"] = "Offer letter created successfully.";
                    return RedirectToAction(nameof(OfferLetters));
                }
                ViewBag.Candidates = await _recruitmentService.GetAllCandidatesAsync();
                ViewBag.JobPositions = await _recruitmentService.GetAllJobPositionsAsync();
                return View(offerLetter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating offer letter");
                TempData["ErrorMessage"] = "Failed to create offer letter.";
                ViewBag.Candidates = await _recruitmentService.GetAllCandidatesAsync();
                ViewBag.JobPositions = await _recruitmentService.GetAllJobPositionsAsync();
                return View(offerLetter);
            }
        }

        // GET: Recruitment/WorkforcePlanning
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> WorkforcePlanning()
        {
            try
            {
                var plans = await _recruitmentService.GetAllWorkforcePlanningsAsync();
                return View(plans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving workforce plans");
                TempData["ErrorMessage"] = "Failed to load workforce plans.";
                return View(new List<WorkforcePlanning>());
            }
        }

        // GET: Recruitment/CreateWorkforcePlan
        [Authorize(Policy = "AdminOnly")]
        public IActionResult CreateWorkforcePlan()
        {
            return View();
        }

        // POST: Recruitment/CreateWorkforcePlan
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CreateWorkforcePlan(WorkforcePlanning plan)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _recruitmentService.CreateWorkforcePlanningAsync(plan);
                    _logger.LogInformation("Created workforce plan by user {User}", User.Identity.Name);
                    
                    TempData["SuccessMessage"] = "Workforce plan created successfully.";
                    return RedirectToAction(nameof(WorkforcePlanning));
                }
                return View(plan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating workforce plan");
                TempData["ErrorMessage"] = "Failed to create workforce plan.";
                return View(plan);
            }
        }

        // GET: Recruitment/JobBoard (Public)
        [AllowAnonymous]
        public async Task<IActionResult> JobBoard()
        {
            try
            {
                var positions = await _recruitmentService.GetAllJobPositionsAsync();
                // Filter active positions (Status == Open)
                var activePositions = positions.Where(p => p.Status == JobStatus.Open).ToList();
                return View(activePositions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving job board positions");
                return View(new List<JobPosition>());
            }
        }

        // GET: Recruitment/ApplyForJob/5 (Public)
        [AllowAnonymous]
        public async Task<IActionResult> ApplyForJob(int id)
        {
            try
            {
                var position = await _recruitmentService.GetJobPositionByIdAsync(id);
                if (position == null || position.Status != JobStatus.Open)
                {
                    return NotFound();
                }
                return View(position);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving job position for application with ID {id}");
                return RedirectToAction(nameof(JobBoard));
            }
        }
    }
}