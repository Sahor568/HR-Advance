using HR_Management_System.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HR_Management_System.Services.Interfaces
{
    public interface IHRISService
    {
        // Timesheet Management
        Task<Timesheet> CreateTimesheetAsync(Timesheet timesheet);
        Task<Timesheet> GetTimesheetByIdAsync(int id);
        Task<IEnumerable<Timesheet>> GetAllTimesheetsAsync(int? employeeId = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<Timesheet> UpdateTimesheetAsync(int id, Timesheet timesheet);
        Task<bool> DeleteTimesheetAsync(int id);
        Task<bool> SubmitTimesheetForApprovalAsync(int timesheetId);
        Task<bool> ApproveTimesheetAsync(int timesheetId, string approvedBy, string comments = null);
        Task<bool> RejectTimesheetAsync(int timesheetId, string rejectedBy, string reason);
        Task<object> GetTimesheetSummaryAsync(int employeeId, int month, int year);
        Task<object> GetTimesheetAnalyticsAsync(int? departmentId = null, DateTime? startDate = null, DateTime? endDate = null);
        
        // Training Management
        Task<Training> CreateTrainingAsync(Training training);
        Task<Training> GetTrainingByIdAsync(int id);
        Task<IEnumerable<Training>> GetAllTrainingsAsync(string status = null, string category = null);
        Task<Training> UpdateTrainingAsync(int id, Training training);
        Task<bool> DeleteTrainingAsync(int id);
        Task<bool> RegisterEmployeeForTrainingAsync(int trainingId, int employeeId);
        Task<bool> CancelTrainingRegistrationAsync(int trainingId, int employeeId);
        Task<IEnumerable<TrainingAttendance>> GetTrainingAttendeesAsync(int trainingId);
        Task<bool> MarkAttendanceAsync(int trainingId, int employeeId, bool attended, string feedback = null);
        Task<bool> SubmitTrainingEvaluationAsync(int trainingId, int employeeId, TrainingEvaluation evaluation);
        Task<object> GetTrainingAnalyticsAsync(int? year = null);
        Task<object> GetTrainingEffectivenessAsync(int trainingId);
        
        // Travel Management
        Task<TravelRequest> CreateTravelRequestAsync(TravelRequest travelRequest);
        Task<TravelRequest> GetTravelRequestByIdAsync(int id);
        Task<IEnumerable<TravelRequest>> GetAllTravelRequestsAsync(int? employeeId = null, string status = null);
        Task<TravelRequest> UpdateTravelRequestAsync(int id, TravelRequest travelRequest);
        Task<bool> DeleteTravelRequestAsync(int id);
        Task<bool> SubmitTravelRequestForApprovalAsync(int travelRequestId);
        Task<bool> ApproveTravelRequestAsync(int travelRequestId, string approvedBy, string level, string comments = null);
        Task<bool> RejectTravelRequestAsync(int travelRequestId, string rejectedBy, string reason);
        Task<bool> CompleteTravelRequestAsync(int travelRequestId, List<TravelExpense> expenses);
        Task<IEnumerable<TravelExpense>> GetTravelExpensesAsync(int travelRequestId);
        Task<object> GetTravelAnalyticsAsync(int? year = null);
        
        // Reimbursement Management
        Task<Reimbursement> CreateReimbursementAsync(Reimbursement reimbursement);
        Task<Reimbursement> GetReimbursementByIdAsync(int id);
        Task<IEnumerable<Reimbursement>> GetAllReimbursementsAsync(int? employeeId = null, string status = null);
        Task<Reimbursement> UpdateReimbursementAsync(int id, Reimbursement reimbursement);
        Task<bool> DeleteReimbursementAsync(int id);
        Task<bool> SubmitReimbursementForApprovalAsync(int reimbursementId);
        Task<bool> ApproveReimbursementAsync(int reimbursementId, string approvedBy, string level, string comments = null);
        Task<bool> RejectReimbursementAsync(int reimbursementId, string rejectedBy, string reason);
        Task<bool> ProcessReimbursementPaymentAsync(int reimbursementId, string paymentMethod, string transactionReference);
        Task<IEnumerable<ReimbursementItem>> GetReimbursementItemsAsync(int reimbursementId);
        Task<object> GetReimbursementAnalyticsAsync(int? year = null);
        
        // Notification Management
        Task<Notification> CreateNotificationAsync(Notification notification);
        Task<Notification> GetNotificationByIdAsync(int id);
        Task<IEnumerable<Notification>> GetAllNotificationsAsync(string userId = null, bool unreadOnly = false);
        Task<Notification> UpdateNotificationAsync(int id, Notification notification);
        Task<bool> DeleteNotificationAsync(int id);
        Task<bool> MarkNotificationAsReadAsync(int notificationId);
        Task<bool> MarkAllNotificationsAsReadAsync(string userId);
        Task<int> GetUnreadNotificationCountAsync(string userId);
        Task<bool> SendBulkNotificationAsync(List<string> userIds, string title, string message, string type = "Info");
        
        // Document Management
        Task<bool> UploadEmployeeDocumentAsync(int employeeId, string documentType, string filePath, string fileName, string description = null);
        Task<IEnumerable<object>> GetEmployeeDocumentsAsync(int employeeId, string documentType = null);
        Task<bool> DeleteEmployeeDocumentAsync(int documentId);
        Task<object> DownloadEmployeeDocumentAsync(int documentId);
        
        // HRIS Analytics
        Task<object> GetHRISDashboardAsync();
        Task<object> GetEmployeeEngagementMetricsAsync(int? departmentId = null);
        Task<object> GetTrainingROIAnalysisAsync(int? year = null);
        Task<object> GetTravelCostAnalysisAsync(int? year = null);
        Task<object> GetReimbursementTrendAnalysisAsync(int? year = null);
        
        // Compliance & Reporting
        Task<object> GenerateHRISReportAsync(string reportType, DateTime startDate, DateTime endDate, string format = "PDF");
        Task<object> GetComplianceStatusAsync();
        Task<bool> ExportHRISDataAsync(string dataType, DateTime startDate, DateTime endDate, string format = "Excel");
    }
}