using HR_Management_System.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HR_Management_System.Services.Interfaces
{
    public interface IEmployeeSelfService
    {
        // Employee Dashboard
        Task<object> GetEmployeeDashboardAsync(int employeeId);
        Task<object> GetUpcomingEventsAsync(int employeeId);
        Task<object> GetPendingApprovalsAsync(int employeeId);
        Task<object> GetRecentActivitiesAsync(int employeeId);
        
        // Personal Information Management
        Task<Employee> GetEmployeeProfileAsync(int employeeId);
        Task<bool> UpdatePersonalInformationAsync(int employeeId, Dictionary<string, object> updates);
        Task<bool> UpdateContactInformationAsync(int employeeId, string phone, string email, string address);
        Task<bool> UploadDocumentAsync(int employeeId, string documentType, string filePath, string fileName);
        Task<IEnumerable<object>> GetEmployeeDocumentsAsync(int employeeId);
        
        // Leave Management (Self Service)
        Task<LeaveRequest> ApplyForLeaveAsync(int employeeId, LeaveRequest leaveRequest);
        Task<IEnumerable<LeaveRequest>> GetMyLeaveRequestsAsync(int employeeId);
        Task<LeaveRequest> GetLeaveRequestDetailsAsync(int employeeId, int leaveRequestId);
        Task<bool> CancelLeaveRequestAsync(int employeeId, int leaveRequestId, string reason);
        Task<object> GetMyLeaveBalanceAsync(int employeeId);
        
        // Attendance & Timesheet (Self Service)
        Task<bool> ClockInAsync(int employeeId, string location = null, string deviceId = null);
        Task<bool> ClockOutAsync(int employeeId, string location = null, string deviceId = null);
        Task<object> GetMyAttendanceSummaryAsync(int employeeId, int month, int year);
        Task<IEnumerable<Attendance>> GetMyAttendanceRecordsAsync(int employeeId, DateTime startDate, DateTime endDate);
        Task<Timesheet> SubmitTimesheetAsync(int employeeId, Timesheet timesheet);
        Task<IEnumerable<Timesheet>> GetMyTimesheetsAsync(int employeeId, int month, int year);
        Task<bool> EditTimesheetAsync(int employeeId, int timesheetId, Timesheet updates);
        
        // Training & Development (Self Service)
        Task<IEnumerable<Training>> GetAvailableTrainingsAsync(int employeeId);
        Task<bool> RegisterForTrainingAsync(int employeeId, int trainingId);
        Task<IEnumerable<TrainingAttendance>> GetMyTrainingRegistrationsAsync(int employeeId);
        Task<bool> SubmitTrainingFeedbackAsync(int employeeId, int trainingId, TrainingEvaluation feedback);
        
        // Travel & Reimbursement (Self Service)
        Task<TravelRequest> SubmitTravelRequestAsync(int employeeId, TravelRequest travelRequest);
        Task<IEnumerable<TravelRequest>> GetMyTravelRequestsAsync(int employeeId);
        Task<bool> CancelTravelRequestAsync(int employeeId, int travelRequestId, string reason);
        Task<Reimbursement> SubmitReimbursementClaimAsync(int employeeId, Reimbursement reimbursement);
        Task<IEnumerable<Reimbursement>> GetMyReimbursementsAsync(int employeeId);
        
        // Performance (Self Service)
        Task<IEnumerable<Performance.PerformanceReview>> GetMyPerformanceReviewsAsync(int employeeId);
        Task<IEnumerable<Performance.PerformanceGoal>> GetMyPerformanceGoalsAsync(int employeeId);
        Task<bool> SubmitSelfAppraisalAsync(int employeeId, int reviewId, Performance.PerformanceFeedback feedback);
        
        // Notifications & Alerts
        Task<IEnumerable<Notification>> GetMyNotificationsAsync(int employeeId, bool unreadOnly = false);
        Task<bool> MarkNotificationAsReadAsync(int employeeId, int notificationId);
        Task<bool> MarkAllNotificationsAsReadAsync(int employeeId);
        Task<int> GetUnreadNotificationCountAsync(int employeeId);
        
        // Approval Workflow (As Approver)
        Task<IEnumerable<object>> GetPendingMyApprovalsAsync(int employeeId);
        Task<bool> ApproveRequestAsync(int employeeId, string requestType, int requestId, string comments);
        Task<bool> RejectRequestAsync(int employeeId, string requestType, int requestId, string reason);
        Task<bool> DelegateApprovalAsync(int employeeId, string requestType, int requestId, int delegateToEmployeeId);
        
        // Employee Exit (Self Service)
        Task<bool> SubmitResignationAsync(int employeeId, EmployeeExit resignation);
        Task<EmployeeExit> GetMyResignationStatusAsync(int employeeId);
        Task<bool> WithdrawResignationAsync(int employeeId, int exitId, string reason);
        Task<ExitSurvey> SubmitExitSurveyAsync(int employeeId, int exitId, ExitSurvey survey);
        
        // Settings & Preferences
        Task<bool> UpdateNotificationPreferencesAsync(int employeeId, Dictionary<string, bool> preferences);
        Task<bool> UpdatePasswordAsync(int employeeId, string currentPassword, string newPassword);
        Task<bool> SetTwoFactorAuthenticationAsync(int employeeId, bool enable);
        
        // Help & Support
        Task<bool> SubmitHelpTicketAsync(int employeeId, string category, string subject, string description);
        Task<IEnumerable<object>> GetMyHelpTicketsAsync(int employeeId);
        Task<object> GetKnowledgeBaseArticlesAsync(string category = null);
    }
}