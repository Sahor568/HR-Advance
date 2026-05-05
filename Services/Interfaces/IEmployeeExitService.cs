using HR_Management_System.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HR_Management_System.Services.Interfaces
{
    public interface IEmployeeExitService
    {
        // Employee Exit Process
        Task<EmployeeExit> InitiateExitProcessAsync(EmployeeExit exit);
        Task<EmployeeExit> GetExitProcessByIdAsync(int id);
        Task<IEnumerable<EmployeeExit>> GetAllExitProcessesAsync(string status = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<EmployeeExit> UpdateExitProcessAsync(int id, EmployeeExit exit);
        Task<bool> DeleteExitProcessAsync(int id);
        Task<bool> ApproveExitProcessAsync(int id, string approvedBy, string comments = null);
        Task<bool> RejectExitProcessAsync(int id, string rejectedBy, string reason);
        Task<bool> CancelExitProcessAsync(int id, string reason);
        
        // Exit Clearance Management
        Task<ExitClearance> CreateExitClearanceAsync(ExitClearance clearance);
        Task<ExitClearance> GetExitClearanceByIdAsync(int id);
        Task<IEnumerable<ExitClearance>> GetExitClearancesByExitIdAsync(int exitId);
        Task<ExitClearance> UpdateExitClearanceAsync(int id, ExitClearance clearance);
        Task<bool> DeleteExitClearanceAsync(int id);
        Task<bool> CompleteExitClearanceAsync(int clearanceId, string clearedBy, string remarks = null);
        Task<bool> WaiveExitClearanceAsync(int clearanceId, string waivedBy, string reason);
        Task<object> GetExitClearanceStatusAsync(int exitId);
        
        // Exit Survey Management
        Task<ExitSurvey> CreateExitSurveyAsync(ExitSurvey survey);
        Task<ExitSurvey> GetExitSurveyByExitIdAsync(int exitId);
        Task<ExitSurvey> UpdateExitSurveyAsync(int id, ExitSurvey survey);
        Task<bool> DeleteExitSurveyAsync(int id);
        Task<bool> SubmitExitSurveyAsync(int exitId, ExitSurvey survey);
        Task<object> GetExitSurveyAnalyticsAsync(int? year = null, string department = null);
        
        // Exit Document Management
        Task<ExitDocument> CreateExitDocumentAsync(ExitDocument document);
        Task<ExitDocument> GetExitDocumentByIdAsync(int id);
        Task<IEnumerable<ExitDocument>> GetExitDocumentsByExitIdAsync(int exitId);
        Task<ExitDocument> UpdateExitDocumentAsync(int id, ExitDocument document);
        Task<bool> DeleteExitDocumentAsync(int id);
        Task<bool> UploadExitDocumentAsync(int exitId, string documentType, string filePath, string fileName, string description = null);
        Task<object> DownloadExitDocumentAsync(int documentId);
        
        // Full & Final Settlement
        Task<object> CalculateFullAndFinalSettlementAsync(int exitId);
        Task<object> GetSettlementBreakdownAsync(int exitId);
        Task<bool> ApproveSettlementAsync(int exitId, string approvedBy, string comments = null);
        Task<bool> ProcessSettlementPaymentAsync(int exitId, string paymentMethod, string transactionReference);
        Task<object> GenerateSettlementLetterAsync(int exitId, string format = "PDF");
        
        // Notice Period Management
        Task<bool> CalculateNoticePeriodAsync(int exitId);
        Task<bool> WaiveNoticePeriodAsync(int exitId, int waivedDays, string waivedBy, string reason);
        Task<bool> BuyoutNoticePeriodAsync(int exitId, decimal buyoutAmount, string approvedBy);
        Task<object> GetNoticePeriodStatusAsync(int exitId);
        
        // Knowledge Transfer
        Task<bool> InitiateKnowledgeTransferAsync(int exitId, int successorId, string transferPlan);
        Task<object> GetKnowledgeTransferStatusAsync(int exitId);
        Task<bool> CompleteKnowledgeTransferAsync(int exitId, string completedBy, string feedback);
        
        // Exit Interview
        Task<bool> ScheduleExitInterviewAsync(int exitId, DateTime interviewDate, string interviewerId, string agenda);
        Task<object> GetExitInterviewDetailsAsync(int exitId);
        Task<bool> ConductExitInterviewAsync(int exitId, string feedback, string recommendations);
        
        // Asset Recovery
        Task<bool> InitiateAssetRecoveryAsync(int exitId, List<string> assets);
        Task<object> GetAssetRecoveryStatusAsync(int exitId);
        Task<bool> CompleteAssetRecoveryAsync(int exitId, string completedBy, string remarks);
        
        // Exit Analytics & Reporting
        Task<object> GetExitProcessAnalyticsAsync(int? year = null, string department = null);
        Task<object> GetTurnoverRateAnalysisAsync(DateTime startDate, DateTime endDate, string dimension = "department");
        Task<object> GetExitReasonAnalysisAsync(int? year = null);
        Task<object> GetRetentionRiskAnalysisAsync();
        Task<object> GenerateExitProcessReportAsync(string reportType, DateTime startDate, DateTime endDate, string format = "PDF");
        
        // Rehire Eligibility
        Task<object> CheckRehireEligibilityAsync(int employeeId);
        Task<bool> UpdateRehireStatusAsync(int employeeId, bool eligible, string reason, DateTime? eligibleFrom = null);
        Task<object> GetRehireEligibilityListAsync(bool eligibleOnly = true);
        
        // Alumni Management
        Task<bool> AddToAlumniDatabaseAsync(int exitId);
        Task<object> GetAlumniDetailsAsync(int employeeId);
        Task<bool> UpdateAlumniInformationAsync(int employeeId, Dictionary<string, object> updates);
        Task<IEnumerable<object>> SearchAlumniAsync(string searchTerm);
        
        // Compliance & Legal
        Task<bool> VerifyExitComplianceAsync(int exitId);
        Task<object> GenerateExitComplianceReportAsync(int exitId);
        Task<bool> ArchiveExitRecordsAsync(int exitId, int retentionYears = 7);
        
        // Workflow Automation
        Task<bool> TriggerExitWorkflowAsync(int exitId, string workflowStep);
        Task<object> GetExitWorkflowStatusAsync(int exitId);
        Task<bool> EscalateExitProcessAsync(int exitId, string escalationReason, string escalatedTo);
    }
}