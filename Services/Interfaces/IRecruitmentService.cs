using HR_Management_System.Models.Recruitment;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HR_Management_System.Services.Interfaces
{
    public interface IRecruitmentService
    {
        // Workforce Planning
        Task<WorkforcePlanning> CreateWorkforcePlanningAsync(WorkforcePlanning planning);
        Task<WorkforcePlanning> GetWorkforcePlanningByIdAsync(int id);
        Task<IEnumerable<WorkforcePlanning>> GetAllWorkforcePlanningsAsync();
        Task<WorkforcePlanning> UpdateWorkforcePlanningAsync(int id, WorkforcePlanning planning);
        Task<bool> DeleteWorkforcePlanningAsync(int id);
        Task<bool> ApproveWorkforcePlanningAsync(int id, string approvedBy);
        
        // Employee Requisition
        Task<EmployeeRequisition> CreateEmployeeRequisitionAsync(EmployeeRequisition requisition);
        Task<EmployeeRequisition> GetEmployeeRequisitionByIdAsync(int id);
        Task<IEnumerable<EmployeeRequisition>> GetAllEmployeeRequisitionsAsync();
        Task<EmployeeRequisition> UpdateEmployeeRequisitionAsync(int id, EmployeeRequisition requisition);
        Task<bool> DeleteEmployeeRequisitionAsync(int id);
        Task<bool> ApproveEmployeeRequisitionAsync(int id, string approvedBy, string level);
        
        // Job Position
        Task<JobPosition> CreateJobPositionAsync(JobPosition position);
        Task<JobPosition> GetJobPositionByIdAsync(int id);
        Task<IEnumerable<JobPosition>> GetAllJobPositionsAsync();
        Task<JobPosition> UpdateJobPositionAsync(int id, JobPosition position);
        Task<bool> DeleteJobPositionAsync(int id);
        Task<IEnumerable<JobPosition>> GetJobPositionsByRequisitionAsync(int requisitionId);
        
        // Candidate Management
        Task<Candidate> CreateCandidateAsync(Candidate candidate);
        Task<Candidate> GetCandidateByIdAsync(int id);
        Task<IEnumerable<Candidate>> GetAllCandidatesAsync();
        Task<Candidate> UpdateCandidateAsync(int id, Candidate candidate);
        Task<bool> DeleteCandidateAsync(int id);
        Task<IEnumerable<Candidate>> GetCandidatesByJobPositionAsync(int jobPositionId);
        Task<bool> UpdateCandidateStatusAsync(int candidateId, string status);
        
        // Interview Management
        Task<Interview> ScheduleInterviewAsync(Interview interview);
        Task<Interview> GetInterviewByIdAsync(int id);
        Task<IEnumerable<Interview>> GetAllInterviewsAsync();
        Task<Interview> UpdateInterviewAsync(int id, Interview interview);
        Task<bool> DeleteInterviewAsync(int id);
        Task<IEnumerable<Interview>> GetInterviewsByCandidateAsync(int candidateId);
        Task<bool> UpdateInterviewResultAsync(int interviewId, string result, string recommendation, string feedback);
        
        // Offer Letter Management
        Task<OfferLetter> CreateOfferLetterAsync(OfferLetter offer);
        Task<OfferLetter> GetOfferLetterByIdAsync(int id);
        Task<IEnumerable<OfferLetter>> GetAllOfferLettersAsync();
        Task<OfferLetter> UpdateOfferLetterAsync(int id, OfferLetter offer);
        Task<bool> DeleteOfferLetterAsync(int id);
        Task<bool> SendOfferLetterAsync(int offerId);
        Task<bool> AcceptOfferLetterAsync(int offerId);
        Task<bool> RejectOfferLetterAsync(int offerId, string reason);
        
        // Recruitment Analytics
        Task<object> GetRecruitmentAnalyticsAsync(int? year = null);
        Task<object> GetTimeToHireMetricsAsync(int? departmentId = null);
        Task<object> GetSourceEffectivenessAsync(int? year = null);
        Task<object> GetCostPerHireAsync(int? year = null);
    }
}