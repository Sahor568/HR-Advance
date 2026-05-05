using HR_Management_System.Models.Performance;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HR_Management_System.Services.Interfaces
{
    public interface IPerformanceService
    {
        // Performance Review
        Task<PerformanceReview> CreatePerformanceReviewAsync(PerformanceReview review);
        Task<PerformanceReview> GetPerformanceReviewByIdAsync(int id);
        Task<IEnumerable<PerformanceReview>> GetAllPerformanceReviewsAsync();
        Task<PerformanceReview> UpdatePerformanceReviewAsync(int id, PerformanceReview review);
        Task<bool> DeletePerformanceReviewAsync(int id);
        Task<IEnumerable<PerformanceReview>> GetPerformanceReviewsByEmployeeAsync(int employeeId);
        Task<IEnumerable<PerformanceReview>> GetPerformanceReviewsByPeriodAsync(int periodId);
        Task<bool> StartPerformanceReviewAsync(int reviewId);
        Task<bool> CompletePerformanceReviewAsync(int reviewId);
        
        // Performance Goal
        Task<PerformanceGoal> CreatePerformanceGoalAsync(PerformanceGoal goal);
        Task<PerformanceGoal> GetPerformanceGoalByIdAsync(int id);
        Task<IEnumerable<PerformanceGoal>> GetAllPerformanceGoalsAsync();
        Task<PerformanceGoal> UpdatePerformanceGoalAsync(int id, PerformanceGoal goal);
        Task<bool> DeletePerformanceGoalAsync(int id);
        Task<IEnumerable<PerformanceGoal>> GetPerformanceGoalsByEmployeeAsync(int employeeId);
        Task<IEnumerable<PerformanceGoal>> GetPerformanceGoalsByReviewAsync(int reviewId);
        Task<bool> UpdateGoalProgressAsync(int goalId, decimal progress, string comments);
        Task<bool> CompletePerformanceGoalAsync(int goalId);
        
        // Performance Feedback (360-degree) how a employee 
        Task<PerformanceFeedback> CreatePerformanceFeedbackAsync(PerformanceFeedback feedback);
        Task<PerformanceFeedback> GetPerformanceFeedbackByIdAsync(int id);
        Task<IEnumerable<PerformanceFeedback>> GetAllPerformanceFeedbacksAsync();
        Task<PerformanceFeedback> UpdatePerformanceFeedbackAsync(int id, PerformanceFeedback feedback);
        Task<bool> DeletePerformanceFeedbackAsync(int id);
        Task<IEnumerable<PerformanceFeedback>> GetFeedbacksByReviewAsync(int reviewId);
        Task<IEnumerable<PerformanceFeedback>> GetFeedbacksByEmployeeAsync(int employeeId);
        Task<IEnumerable<PerformanceFeedback>> GetFeedbacksByReviewerAsync(string reviewerId);
        Task<bool> SubmitFeedbackAsync(int feedbackId);
        
        // Online Exam
        Task<OnlineExam> CreateOnlineExamAsync(OnlineExam exam);
        Task<OnlineExam> GetOnlineExamByIdAsync(int id);
        Task<IEnumerable<OnlineExam>> GetAllOnlineExamsAsync();
        Task<OnlineExam> UpdateOnlineExamAsync(int id, OnlineExam exam);
        Task<bool> DeleteOnlineExamAsync(int id);
        Task<IEnumerable<OnlineExam>> GetExamsByEmployeeAsync(int employeeId);
        Task<IEnumerable<OnlineExam>> GetExamsByReviewAsync(int reviewId);
        Task<bool> StartExamAttemptAsync(int examId, int employeeId);
        Task<ExamAttempt> SubmitExamAnswersAsync(int attemptId, Dictionary<int, string> answers);
        Task<ExamAttempt> EvaluateExamAttemptAsync(int attemptId);
        
        // Performance Analytics
        Task<object> GetPerformanceAnalyticsAsync(int? employeeId = null, int? year = null);
        Task<object> GetGoalCompletionRateAsync(int? departmentId = null, int? year = null);
        Task<object> GetFeedbackDistributionAsync(int reviewId);
        Task<object> GetCompetencyGapAnalysisAsync(int employeeId);
        Task<object> GetPerformanceTrendAsync(int employeeId, int years = 3);
        
        // Performance Calibration
        Task<bool> CalibratePerformanceRatingsAsync(int reviewId);
        Task<object> GetPerformanceDistributionAsync(int reviewId);
        Task<bool> ApplyForcedRankingAsync(int reviewId, string distributionType);
        
        // Development Planning
        Task<object> CreateDevelopmentPlanAsync(int employeeId, int reviewId, string planDetails);
        Task<object> GetDevelopmentPlanAsync(int employeeId);
        Task<bool> UpdateDevelopmentPlanProgressAsync(int planId, decimal progress, string comments);
    }
}