using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HR_Management_System.Data;
using HR_Management_System.Models.Recruitment;
using HR_Management_System.Services.Interfaces;

namespace HR_Management_System.Services.Implementations
{
    public class RecruitmentService : IRecruitmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RecruitmentService> _logger;

        public RecruitmentService(ApplicationDbContext context, ILogger<RecruitmentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Workforce Planning

        public async Task<WorkforcePlanning> CreateWorkforcePlanningAsync(WorkforcePlanning planning)
        {
            _logger.LogInformation("Creating workforce planning for department: {Department}", planning.Department);
            planning.CreatedDate = DateTime.UtcNow;
            planning.Status = "Draft";
            _context.WorkforcePlannings.Add(planning);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Workforce planning created with ID: {Id}", planning.Id);
            return planning;
        }

        public async Task<WorkforcePlanning> GetWorkforcePlanningByIdAsync(int id)
        {
            _logger.LogInformation("Fetching workforce planning with ID: {Id}", id);
            return await _context.WorkforcePlannings.FindAsync(id);
        }

        public async Task<IEnumerable<WorkforcePlanning>> GetAllWorkforcePlanningsAsync()
        {
            _logger.LogInformation("Fetching all workforce plannings");
            return await _context.WorkforcePlannings
                .OrderByDescending(w => w.CreatedDate)
                .ToListAsync();
        }

        public async Task<WorkforcePlanning> UpdateWorkforcePlanningAsync(int id, WorkforcePlanning planning)
        {
            _logger.LogInformation("Updating workforce planning with ID: {Id}", id);
            var existing = await _context.WorkforcePlannings.FindAsync(id);
            if (existing == null)
            {
                _logger.LogWarning("Workforce planning not found with ID: {Id}", id);
                return null;
            }

            existing.Department = planning.Department;
            existing.FiscalYear = planning.FiscalYear;
            existing.CurrentHeadcount = planning.CurrentHeadcount;
            existing.RequiredHeadcount = planning.RequiredHeadcount;
            existing.BudgetAllocation = planning.BudgetAllocation;
            existing.Justification = planning.Justification;
            existing.TargetDate = planning.TargetDate;
            existing.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Workforce planning updated with ID: {Id}", id);
            return existing;
        }

        public async Task<bool> DeleteWorkforcePlanningAsync(int id)
        {
            _logger.LogInformation("Deleting workforce planning with ID: {Id}", id);
            var planning = await _context.WorkforcePlannings.FindAsync(id);
            if (planning == null) return false;

            _context.WorkforcePlannings.Remove(planning);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Workforce planning deleted with ID: {Id}", id);
            return true;
        }

        public async Task<bool> ApproveWorkforcePlanningAsync(int id, string approvedBy)
        {
            _logger.LogInformation("Approving workforce planning with ID: {Id} by {ApprovedBy}", id, approvedBy);
            var planning = await _context.WorkforcePlannings.FindAsync(id);
            if (planning == null) return false;

            planning.Status = "Approved";
            planning.ApprovedBy = approvedBy;
            planning.ApprovedDate = DateTime.UtcNow;
            planning.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Workforce planning approved with ID: {Id}", id);
            return true;
        }

        #endregion

        #region Employee Requisition

        public async Task<EmployeeRequisition> CreateEmployeeRequisitionAsync(EmployeeRequisition requisition)
        {
            _logger.LogInformation("Creating employee requisition for position: {Position}", requisition.PositionTitle);
            requisition.CreatedDate = DateTime.UtcNow;
            requisition.Status = "Pending";
            _context.EmployeeRequisitions.Add(requisition);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Employee requisition created with ID: {Id}", requisition.Id);
            return requisition;
        }

        public async Task<EmployeeRequisition> GetEmployeeRequisitionByIdAsync(int id)
        {
            _logger.LogInformation("Fetching employee requisition with ID: {Id}", id);
            return await _context.EmployeeRequisitions.FindAsync(id);
        }

        public async Task<IEnumerable<EmployeeRequisition>> GetAllEmployeeRequisitionsAsync()
        {
            _logger.LogInformation("Fetching all employee requisitions");
            return await _context.EmployeeRequisitions
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        public async Task<EmployeeRequisition> UpdateEmployeeRequisitionAsync(int id, EmployeeRequisition requisition)
        {
            _logger.LogInformation("Updating employee requisition with ID: {Id}", id);
            var existing = await _context.EmployeeRequisitions.FindAsync(id);
            if (existing == null) return null;

            existing.PositionTitle = requisition.PositionTitle;
            existing.Department = requisition.Department;
            existing.NumberOfPositions = requisition.NumberOfPositions;
            existing.EmploymentType = requisition.EmploymentType;
            existing.SalaryRangeMin = requisition.SalaryRangeMin;
            existing.SalaryRangeMax = requisition.SalaryRangeMax;
            existing.JobDescription = requisition.JobDescription;
            existing.RequiredQualifications = requisition.RequiredQualifications;
            existing.RequiredExperience = requisition.RequiredExperience;
            existing.UrgencyLevel = requisition.UrgencyLevel;
            existing.TargetDate = requisition.TargetDate;
            existing.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteEmployeeRequisitionAsync(int id)
        {
            _logger.LogInformation("Deleting employee requisition with ID: {Id}", id);
            var requisition = await _context.EmployeeRequisitions.FindAsync(id);
            if (requisition == null) return false;

            _context.EmployeeRequisitions.Remove(requisition);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ApproveEmployeeRequisitionAsync(int id, string approvedBy, string level)
        {
            _logger.LogInformation("Approving employee requisition ID: {Id} at level: {Level} by {ApprovedBy}", id, level, approvedBy);
            var requisition = await _context.EmployeeRequisitions.FindAsync(id);
            if (requisition == null) return false;

            if (level == "DepartmentHead")
            {
                requisition.DepartmentHeadApproval = true;
                requisition.DepartmentHeadApprovedBy = approvedBy;
                requisition.DepartmentHeadApprovedDate = DateTime.UtcNow;
                requisition.Status = "DepartmentApproved";
            }
            else if (level == "HR")
            {
                requisition.HRApproval = true;
                requisition.HRApprovedBy = approvedBy;
                requisition.HRApprovedDate = DateTime.UtcNow;
                requisition.Status = "HRApproved";
            }
            else if (level == "Management")
            {
                requisition.ManagementApproval = true;
                requisition.ManagementApprovedBy = approvedBy;
                requisition.ManagementApprovedDate = DateTime.UtcNow;
                requisition.Status = "FullyApproved";
            }

            requisition.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Job Position

        public async Task<JobPosition> CreateJobPositionAsync(JobPosition position)
        {
            _logger.LogInformation("Creating job position: {Title}", position.Title);
            position.CreatedDate = DateTime.UtcNow;
            position.Status = JobStatus.Draft;
            _context.JobPositions.Add(position);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Job position created with ID: {Id}", position.Id);
            return position;
        }

        public async Task<JobPosition> GetJobPositionByIdAsync(int id)
        {
            _logger.LogInformation("Fetching job position with ID: {Id}", id);
            return await _context.JobPositions.FindAsync(id);
        }

        public async Task<IEnumerable<JobPosition>> GetAllJobPositionsAsync()
        {
            _logger.LogInformation("Fetching all job positions");
            return await _context.JobPositions
                .OrderByDescending(j => j.CreatedDate)
                .ToListAsync();
        }

        public async Task<JobPosition> UpdateJobPositionAsync(int id, JobPosition position)
        {
            _logger.LogInformation("Updating job position with ID: {Id}", id);
            var existing = await _context.JobPositions.FindAsync(id);
            if (existing == null) return null;

            existing.Title = position.Title;
            existing.Department = position.Department;
            existing.Description = position.Description;
            existing.Requirements = position.Requirements;
            existing.Responsibilities = position.Responsibilities;
            existing.MinSalary = position.MinSalary;
            existing.MaxSalary = position.MaxSalary;
            existing.PositionType = position.PositionType;
            existing.NoOfVacancies = position.NoOfVacancies;
            existing.ClosingDate = position.ClosingDate;
            existing.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteJobPositionAsync(int id)
        {
            _logger.LogInformation("Deleting job position with ID: {Id}", id);
            var position = await _context.JobPositions.FindAsync(id);
            if (position == null) return false;

            _context.JobPositions.Remove(position);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<JobPosition>> GetJobPositionsByRequisitionAsync(int requisitionId)
        {
            _logger.LogInformation("Fetching job positions for requisition ID: {RequisitionId}", requisitionId);
            return await _context.JobPositions
                .Where(j => j.EmployeeRequisitionId == requisitionId)
                .ToListAsync();
        }

        #endregion

        #region Candidate Management

        public async Task<Candidate> CreateCandidateAsync(Candidate candidate)
        {
            _logger.LogInformation("Creating candidate: {Name} for position: {JobPositionId}", candidate.FullName, candidate.JobPositionId);
            candidate.AppliedDate = DateTime.UtcNow;
            candidate.Status = ApplicationStatus.Applied;
            _context.Candidates.Add(candidate);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Candidate created with ID: {Id}", candidate.Id);
            return candidate;
        }

        public async Task<Candidate> GetCandidateByIdAsync(int id)
        {
            _logger.LogInformation("Fetching candidate with ID: {Id}", id);
            return await _context.Candidates.FindAsync(id);
        }

        public async Task<IEnumerable<Candidate>> GetAllCandidatesAsync()
        {
            _logger.LogInformation("Fetching all candidates");
            return await _context.Candidates
                .OrderByDescending(c => c.AppliedDate)
                .ToListAsync();
        }

        public async Task<Candidate> UpdateCandidateAsync(int id, Candidate candidate)
        {
            _logger.LogInformation("Updating candidate with ID: {Id}", id);
            var existing = await _context.Candidates.FindAsync(id);
            if (existing == null) return null;

            existing.FullName = candidate.FullName;
            existing.Email = candidate.Email;
            existing.Phone = candidate.Phone;
            existing.Address = candidate.Address;
            existing.DateOfBirth = candidate.DateOfBirth;
            existing.Gender = candidate.Gender;
            existing.Nationality = candidate.Nationality;
            existing.Education = candidate.Education;
            existing.Experience = candidate.Experience;
            existing.Skills = candidate.Skills;
            existing.CurrentEmployer = candidate.CurrentEmployer;
            existing.CurrentSalary = candidate.CurrentSalary;
            existing.ExpectedSalary = candidate.ExpectedSalary;
            existing.NoticePeriod = candidate.NoticePeriod;
            existing.Source = candidate.Source;
            existing.ReferralEmployeeId = candidate.ReferralEmployeeId;
            existing.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteCandidateAsync(int id)
        {
            _logger.LogInformation("Deleting candidate with ID: {Id}", id);
            var candidate = await _context.Candidates.FindAsync(id);
            if (candidate == null) return false;

            _context.Candidates.Remove(candidate);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Candidate>> GetCandidatesByJobPositionAsync(int jobPositionId)
        {
            _logger.LogInformation("Fetching candidates for job position ID: {JobPositionId}", jobPositionId);
            return await _context.Candidates
                .Where(c => c.JobPositionId == jobPositionId)
                .OrderByDescending(c => c.AppliedDate)
                .ToListAsync();
        }

        public async Task<bool> UpdateCandidateStatusAsync(int candidateId, string status)
        {
            _logger.LogInformation("Updating candidate ID: {CandidateId} status to: {Status}", candidateId, status);
            var candidate = await _context.Candidates.FindAsync(candidateId);
            if (candidate == null) return false;

            if (Enum.TryParse<ApplicationStatus>(status, out var appStatus))
            {
                candidate.Status = appStatus;
                candidate.ModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }

            _logger.LogWarning("Invalid status value: {Status}", status);
            return false;
        }

        #endregion

        #region Interview Management

        public async Task<Interview> ScheduleInterviewAsync(Interview interview)
        {
            _logger.LogInformation("Scheduling interview for candidate ID: {CandidateId}", interview.CandidateId);
            interview.CreatedDate = DateTime.UtcNow;
            interview.Status = InterviewStatus.Scheduled;
            _context.Interviews.Add(interview);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Interview scheduled with ID: {Id}", interview.Id);
            return interview;
        }

        public async Task<Interview> GetInterviewByIdAsync(int id)
        {
            _logger.LogInformation("Fetching interview with ID: {Id}", id);
            return await _context.Interviews.FindAsync(id);
        }

        public async Task<IEnumerable<Interview>> GetAllInterviewsAsync()
        {
            _logger.LogInformation("Fetching all interviews");
            return await _context.Interviews
                .OrderByDescending(i => i.ScheduledDate)
                .ToListAsync();
        }

        public async Task<Interview> UpdateInterviewAsync(int id, Interview interview)
        {
            _logger.LogInformation("Updating interview with ID: {Id}", id);
            var existing = await _context.Interviews.FindAsync(id);
            if (existing == null) return null;

            existing.ScheduledDate = interview.ScheduledDate;
            existing.ScheduledTime = interview.ScheduledTime;
            existing.InterviewRound = interview.InterviewRound;
            existing.InterviewType = interview.InterviewType;
            existing.InterviewerName = interview.InterviewerName;
            existing.InterviewerId = interview.InterviewerId;
            existing.Location = interview.Location;
            existing.MeetingLink = interview.MeetingLink;
            existing.Notes = interview.Notes;
            existing.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteInterviewAsync(int id)
        {
            _logger.LogInformation("Deleting interview with ID: {Id}", id);
            var interview = await _context.Interviews.FindAsync(id);
            if (interview == null) return false;

            _context.Interviews.Remove(interview);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Interview>> GetInterviewsByCandidateAsync(int candidateId)
        {
            _logger.LogInformation("Fetching interviews for candidate ID: {CandidateId}", candidateId);
            return await _context.Interviews
                .Where(i => i.CandidateId == candidateId)
                .OrderBy(i => i.InterviewRound)
                .ToListAsync();
        }

        public async Task<bool> UpdateInterviewResultAsync(int interviewId, string result, string recommendation, string feedback)
        {
            _logger.LogInformation("Updating interview result for ID: {InterviewId}", interviewId);
            var interview = await _context.Interviews.FindAsync(interviewId);
            if (interview == null) return false;

            if (Enum.TryParse<InterviewStatus>(result, out var status))
                interview.Status = status;

            if (Enum.TryParse<Recommendation>(recommendation, out var rec))
                interview.Recommendation = rec;

            interview.Feedback = feedback;
            interview.CompletedDate = DateTime.UtcNow;
            interview.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Offer Letter Management

        public async Task<OfferLetter> CreateOfferLetterAsync(OfferLetter offer)
        {
            _logger.LogInformation("Creating offer letter for candidate ID: {CandidateId}", offer.CandidateId);
            offer.CreatedDate = DateTime.UtcNow;
            offer.Status = OfferStatus.Draft;
            offer.OfferLetterNumber = $"OL-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}";
            _context.OfferLetters.Add(offer);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Offer letter created with ID: {Id}", offer.Id);
            return offer;
        }

        public async Task<OfferLetter> GetOfferLetterByIdAsync(int id)
        {
            _logger.LogInformation("Fetching offer letter with ID: {Id}", id);
            return await _context.OfferLetters.FindAsync(id);
        }

        public async Task<IEnumerable<OfferLetter>> GetAllOfferLettersAsync()
        {
            _logger.LogInformation("Fetching all offer letters");
            return await _context.OfferLetters
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
        }

        public async Task<OfferLetter> UpdateOfferLetterAsync(int id, OfferLetter offer)
        {
            _logger.LogInformation("Updating offer letter with ID: {Id}", id);
            var existing = await _context.OfferLetters.FindAsync(id);
            if (existing == null) return null;

            existing.Position = offer.Position;
            existing.Department = offer.Department;
            existing.OfferedSalary = offer.OfferedSalary;
            existing.ProbationPeriod = offer.ProbationPeriod;
            existing.StartDate = offer.StartDate;
            existing.EmploymentType = offer.EmploymentType;
            existing.TermsAndConditions = offer.TermsAndConditions;
            existing.Benefits = offer.Benefits;
            existing.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteOfferLetterAsync(int id)
        {
            _logger.LogInformation("Deleting offer letter with ID: {Id}", id);
            var offer = await _context.OfferLetters.FindAsync(id);
            if (offer == null) return false;

            _context.OfferLetters.Remove(offer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SendOfferLetterAsync(int offerId)
        {
            _logger.LogInformation("Sending offer letter ID: {OfferId}", offerId);
            var offer = await _context.OfferLetters.FindAsync(offerId);
            if (offer == null) return false;

            offer.Status = OfferStatus.Sent;
            offer.SentDate = DateTime.UtcNow;
            offer.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Offer letter sent for ID: {OfferId}", offerId);
            return true;
        }

        public async Task<bool> AcceptOfferLetterAsync(int offerId)
        {
            _logger.LogInformation("Accepting offer letter ID: {OfferId}", offerId);
            var offer = await _context.OfferLetters.FindAsync(offerId);
            if (offer == null) return false;

            offer.Status = OfferStatus.Accepted;
            offer.AcceptedDate = DateTime.UtcNow;
            offer.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Offer letter accepted for ID: {OfferId}", offerId);
            return true;
        }

        public async Task<bool> RejectOfferLetterAsync(int offerId, string reason)
        {
            _logger.LogInformation("Rejecting offer letter ID: {OfferId}", offerId);
            var offer = await _context.OfferLetters.FindAsync(offerId);
            if (offer == null) return false;

            offer.Status = OfferStatus.Rejected;
            offer.RejectionReason = reason;
            offer.RejectedDate = DateTime.UtcNow;
            offer.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Offer letter rejected for ID: {OfferId}", offerId);
            return true;
        }

        #endregion

        #region Recruitment Analytics

        public async Task<object> GetRecruitmentAnalyticsAsync(int? year = null)
        {
            _logger.LogInformation("Fetching recruitment analytics for year: {Year}", year);
            var targetYear = year ?? DateTime.UtcNow.Year;

            var totalPositions = await _context.JobPositions
                .CountAsync(j => j.CreatedDate.Year == targetYear);

            var totalCandidates = await _context.Candidates
                .CountAsync(c => c.AppliedDate.Year == targetYear);

            var hiredCandidates = await _context.Candidates
                .CountAsync(c => c.Status == ApplicationStatus.Hired && c.AppliedDate.Year == targetYear);

            var rejectedCandidates = await _context.Candidates
                .CountAsync(c => c.Status == ApplicationStatus.Rejected && c.AppliedDate.Year == targetYear);

            var offersSent = await _context.OfferLetters
                .CountAsync(o => o.CreatedDate.Year == targetYear);

            var offersAccepted = await _context.OfferLetters
                .CountAsync(o => o.Status == OfferStatus.Accepted && o.CreatedDate.Year == targetYear);

            var offersRejected = await _context.OfferLetters
                .CountAsync(o => o.Status == OfferStatus.Rejected && o.CreatedDate.Year == targetYear);

            var candidatesBySource = await _context.Candidates
                .Where(c => c.AppliedDate.Year == targetYear)
                .GroupBy(c => c.Source)
                .Select(g => new { Source = g.Key.ToString(), Count = g.Count() })
                .ToListAsync();

            var candidatesByStatus = await _context.Candidates
                .Where(c => c.AppliedDate.Year == targetYear)
                .GroupBy(c => c.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                .ToListAsync();

            var monthlyApplications = await _context.Candidates
                .Where(c => c.AppliedDate.Year == targetYear)
                .GroupBy(c => c.AppliedDate.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToListAsync();

            return new
            {
                Year = targetYear,
                TotalPositions = totalPositions,
                TotalCandidates = totalCandidates,
                HiredCandidates = hiredCandidates,
                RejectedCandidates = rejectedCandidates,
                OffersSent = offersSent,
                OffersAccepted = offersAccepted,
                OffersRejected = offersRejected,
                HireRate = totalCandidates > 0 ? Math.Round((double)hiredCandidates / totalCandidates * 100, 2) : 0,
                OfferAcceptanceRate = offersSent > 0 ? Math.Round((double)offersAccepted / offersSent * 100, 2) : 0,
                CandidatesBySource = candidatesBySource,
                CandidatesByStatus = candidatesByStatus,
                MonthlyApplications = monthlyApplications
            };
        }

        public async Task<object> GetTimeToHireMetricsAsync(int? departmentId = null)
        {
            _logger.LogInformation("Fetching time-to-hire metrics for department: {DepartmentId}", departmentId);

            var hiredCandidates = await _context.Candidates
                .Where(c => c.Status == ApplicationStatus.Hired)
                .ToListAsync();

            var avgTimeToHire = hiredCandidates.Any()
                ? hiredCandidates
                    .Where(c => c.ModifiedDate.HasValue && c.AppliedDate != default)
                    .Select(c => (c.ModifiedDate.Value - c.AppliedDate).TotalDays)
                    .DefaultIfEmpty(0)
                    .Average()
                : 0;

            var positions = await _context.JobPositions.ToListAsync();
            var openPositions = positions.Count(p => p.Status == JobStatus.Open || p.Status == JobStatus.Published);
            var closedPositions = positions.Count(p => p.Status == JobStatus.Closed || p.Status == JobStatus.Filled);

            return new
            {
                AverageTimeToHireDays = Math.Round(avgTimeToHire, 1),
                OpenPositions = openPositions,
                ClosedPositions = closedPositions,
                TotalPositions = positions.Count,
                HiredCount = hiredCandidates.Count
            };
        }

        public async Task<object> GetSourceEffectivenessAsync(int? year = null)
        {
            _logger.LogInformation("Fetching source effectiveness for year: {Year}", year);
            var targetYear = year ?? DateTime.UtcNow.Year;

            var sourceData = await _context.Candidates
                .Where(c => c.AppliedDate.Year == targetYear)
                .GroupBy(c => c.Source)
                .Select(g => new
                {
                    Source = g.Key.ToString(),
                    TotalApplications = g.Count(),
                    Hired = g.Count(c => c.Status == ApplicationStatus.Hired),
                    Rejected = g.Count(c => c.Status == ApplicationStatus.Rejected),
                    InProgress = g.Count(c => c.Status != ApplicationStatus.Hired && c.Status != ApplicationStatus.Rejected)
                })
                .ToListAsync();

            return sourceData.Select(s => new
            {
                s.Source,
                s.TotalApplications,
                s.Hired,
                s.Rejected,
                s.InProgress,
                ConversionRate = s.TotalApplications > 0
                    ? Math.Round((double)s.Hired / s.TotalApplications * 100, 2)
                    : 0
            });
        }

        public async Task<object> GetCostPerHireAsync(int? year = null)
        {
            _logger.LogInformation("Fetching cost per hire for year: {Year}", year);
            var targetYear = year ?? DateTime.UtcNow.Year;

            var totalHired = await _context.Candidates
                .CountAsync(c => c.Status == ApplicationStatus.Hired && c.AppliedDate.Year == targetYear);

            var totalInterviews = await _context.Interviews
                .CountAsync(i => i.CreatedDate.Year == targetYear);

            var totalOffers = await _context.OfferLetters
                .CountAsync(o => o.CreatedDate.Year == targetYear);

            return new
            {
                Year = targetYear,
                TotalHired = totalHired,
                TotalInterviewsConducted = totalInterviews,
                TotalOffersMade = totalOffers,
                InterviewToHireRatio = totalHired > 0 ? Math.Round((double)totalInterviews / totalHired, 2) : 0,
                OfferToHireRatio = totalHired > 0 ? Math.Round((double)totalOffers / totalHired, 2) : 0
            };
        }

        #endregion
    }
}
