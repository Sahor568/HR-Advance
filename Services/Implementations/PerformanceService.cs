using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HR_Management_System.Data;
using HR_Management_System.Models.Performance;
using HR_Management_System.Services.Interfaces;

namespace HR_Management_System.Services.Implementations
{
    public class PerformanceService : IPerformanceService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PerformanceService> _logger;

        public PerformanceService(ApplicationDbContext context, ILogger<PerformanceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Performance Review

        public async Task<PerformanceReview> CreatePerformanceReviewAsync(PerformanceReview review)
        {
            _logger.LogInformation("Creating performance review for employee ID: {EmployeeId}", review.EmployeeId);
            review.CreatedDate = DateTime.UtcNow;
            review.Status = ReviewStatus.Draft;
            _context.PerformanceReviews.Add(review);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Performance review created with ID: {Id}", review.Id);
            return review;
        }

        public async Task<PerformanceReview> GetPerformanceReviewByIdAsync(int id)
        {
            _logger.LogInformation("Fetching performance review with ID: {Id}", id);
            return await _context.PerformanceReviews.FindAsync(id);
        }

        public async Task<IEnumerable<PerformanceReview>> GetAllPerformanceReviewsAsync()
        {
            _logger.LogInformation("Fetching all performance reviews");
            return await _context.PerformanceReviews
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        public async Task<PerformanceReview> UpdatePerformanceReviewAsync(int id, PerformanceReview review)
        {
            _logger.LogInformation("Updating performance review with ID: {Id}", id);
            var existing = await _context.PerformanceReviews.FindAsync(id);
            if (existing == null) return null;

            existing.ReviewPeriodStart = review.ReviewPeriodStart;
            existing.ReviewPeriodEnd = review.ReviewPeriodEnd;
            existing.ReviewType = review.ReviewType;
            existing.OverallRating = review.OverallRating;
            existing.Comments = review.Comments;
            existing.Strengths = review.Strengths;
            existing.AreasForImprovement = review.AreasForImprovement;
            existing.DevelopmentPlan = review.DevelopmentPlan;
            existing.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeletePerformanceReviewAsync(int id)
        {
            _logger.LogInformation("Deleting performance review with ID: {Id}", id);
            var review = await _context.PerformanceReviews.FindAsync(id);
            if (review == null) return false;

            _context.PerformanceReviews.Remove(review);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<PerformanceReview>> GetPerformanceReviewsByEmployeeAsync(int employeeId)
        {
            _logger.LogInformation("Fetching performance reviews for employee ID: {EmployeeId}", employeeId);
            return await _context.PerformanceReviews
                .Where(r => r.EmployeeId == employeeId)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PerformanceReview>> GetPerformanceReviewsByPeriodAsync(int periodId)
        {
            _logger.LogInformation("Fetching performance reviews for period ID: {PeriodId}", periodId);
            return await _context.PerformanceReviews
                .Where(r => r.ReviewPeriodId == periodId)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        public async Task<bool> StartPerformanceReviewAsync(int reviewId)
        {
            _logger.LogInformation("Starting performance review with ID: {Id}", reviewId);
            var review = await _context.PerformanceReviews.FindAsync(reviewId);
            if (review == null) return false;

            review.Status = ReviewStatus.InProgress;
            review.ReviewStartDate = DateTime.UtcNow;
            review.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CompletePerformanceReviewAsync(int reviewId)
        {
            _logger.LogInformation("Completing performance review with ID: {Id}", reviewId);
            var review = await _context.PerformanceReviews.FindAsync(reviewId);
            if (review == null) return false;

            review.Status = ReviewStatus.Completed;
            review.ReviewEndDate = DateTime.UtcNow;
            review.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Performance Goal

        public async Task<PerformanceGoal> CreatePerformanceGoalAsync(PerformanceGoal goal)
        {
            _logger.LogInformation("Creating performance goal for employee ID: {EmployeeId}", goal.EmployeeId);
            goal.CreatedDate = DateTime.UtcNow;
            goal.Status = GoalStatus.NotStarted;
            _context.PerformanceGoals.Add(goal);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Performance goal created with ID: {Id}", goal.Id);
            return goal;
        }

        public async Task<PerformanceGoal> GetPerformanceGoalByIdAsync(int id)
        {
            _logger.LogInformation("Fetching performance goal with ID: {Id}", id);
            return await _context.PerformanceGoals.FindAsync(id);
        }

        public async Task<IEnumerable<PerformanceGoal>> GetAllPerformanceGoalsAsync()
        {
            _logger.LogInformation("Fetching all performance goals");
            return await _context.PerformanceGoals
                .OrderByDescending(g => g.CreatedDate)
                .ToListAsync();
        }

        public async Task<PerformanceGoal> UpdatePerformanceGoalAsync(int id, PerformanceGoal goal)
        {
            _logger.LogInformation("Updating performance goal with ID: {Id}", id);
            var existing = await _context.PerformanceGoals.FindAsync(id);
            if (existing == null) return null;

            existing.Title = goal.Title;
            existing.Description = goal.Description;
            existing.GoalType = goal.GoalType;
            existing.Category = goal.Category;
            existing.Priority = goal.Priority;
            existing.TargetDate = goal.TargetDate;
            existing.MeasurementCriteria = goal.MeasurementCriteria;
            existing.Weightage = goal.Weightage;
            existing.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeletePerformanceGoalAsync(int id)
        {
            _logger.LogInformation("Deleting performance goal with ID: {Id}", id);
            var goal = await _context.PerformanceGoals.FindAsync(id);
            if (goal == null) return false;

            _context.PerformanceGoals.Remove(goal);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<PerformanceGoal>> GetPerformanceGoalsByEmployeeAsync(int employeeId)
        {
            _logger.LogInformation("Fetching performance goals for employee ID: {EmployeeId}", employeeId);
            return await _context.PerformanceGoals
                .Where(g => g.EmployeeId == employeeId)
                .OrderByDescending(g => g.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PerformanceGoal>> GetPerformanceGoalsByReviewAsync(int reviewId)
        {
            _logger.LogInformation("Fetching performance goals for review ID: {ReviewId}", reviewId);
            return await _context.PerformanceGoals
                .Where(g => g.PerformanceReviewId == reviewId)
                .OrderByDescending(g => g.CreatedDate)
                .ToListAsync();
        }

        public async Task<bool> UpdateGoalProgressAsync(int goalId, decimal progress, string comments)
        {
            _logger.LogInformation("Updating goal progress for ID: {GoalId} to {Progress}%", goalId, progress);
            var goal = await _context.PerformanceGoals.FindAsync(goalId);
            if (goal == null) return false;

            goal.Progress = progress;
            goal.Comments = comments;
            goal.ModifiedDate = DateTime.UtcNow;

            if (progress >= 100)
                goal.Status = GoalStatus.Completed;
            else if (progress > 0)
                goal.Status = GoalStatus.InProgress;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CompletePerformanceGoalAsync(int goalId)
        {
            _logger.LogInformation("Completing performance goal with ID: {GoalId}", goalId);
            var goal = await _context.PerformanceGoals.FindAsync(goalId);
            if (goal == null) return false;

            goal.Status = GoalStatus.Completed;
            goal.Progress = 100;
            goal.ActualCompletionDate = DateTime.UtcNow;
            goal.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Performance Feedback (360-degree)

        public async Task<PerformanceFeedback> CreatePerformanceFeedbackAsync(PerformanceFeedback feedback)
        {
            _logger.LogInformation("Creating performance feedback for review ID: {ReviewId}", feedback.PerformanceReviewId);
            feedback.CreatedDate = DateTime.UtcNow;
            feedback.Status = FeedbackStatus.Draft;
            _context.PerformanceFeedbacks.Add(feedback);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Performance feedback created with ID: {Id}", feedback.Id);
            return feedback;
        }

        public async Task<PerformanceFeedback> GetPerformanceFeedbackByIdAsync(int id)
        {
            _logger.LogInformation("Fetching performance feedback with ID: {Id}", id);
            return await _context.PerformanceFeedbacks.FindAsync(id);
        }

        public async Task<IEnumerable<PerformanceFeedback>> GetAllPerformanceFeedbacksAsync()
        {
            _logger.LogInformation("Fetching all performance feedbacks");
            return await _context.PerformanceFeedbacks
                .OrderByDescending(f => f.CreatedDate)
                .ToListAsync();
        }

        public async Task<PerformanceFeedback> UpdatePerformanceFeedbackAsync(int id, PerformanceFeedback feedback)
        {
            _logger.LogInformation("Updating performance feedback with ID: {Id}", id);
            var existing = await _context.PerformanceFeedbacks.FindAsync(id);
            if (existing == null) return null;

            existing.Rating = feedback.Rating;
            existing.Comments = feedback.Comments;
            existing.Strengths = feedback.Strengths;
            existing.AreasForImprovement = feedback.AreasForImprovement;
            existing.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeletePerformanceFeedbackAsync(int id)
        {
            _logger.LogInformation("Deleting performance feedback with ID: {Id}", id);
            var feedback = await _context.PerformanceFeedbacks.FindAsync(id);
            if (feedback == null) return false;

            _context.PerformanceFeedbacks.Remove(feedback);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<PerformanceFeedback>> GetFeedbacksByReviewAsync(int reviewId)
        {
            _logger.LogInformation("Fetching feedbacks for review ID: {ReviewId}", reviewId);
            return await _context.PerformanceFeedbacks
                .Where(f => f.PerformanceReviewId == reviewId)
                .OrderByDescending(f => f.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PerformanceFeedback>> GetFeedbacksByEmployeeAsync(int employeeId)
        {
            _logger.LogInformation("Fetching feedbacks for employee ID: {EmployeeId}", employeeId);
            return await _context.PerformanceFeedbacks
                .Where(f => f.EmployeeId == employeeId)
                .OrderByDescending(f => f.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PerformanceFeedback>> GetFeedbacksByReviewerAsync(string reviewerId)
        {
            _logger.LogInformation("Fetching feedbacks by reviewer ID: {ReviewerId}", reviewerId);
            return await _context.PerformanceFeedbacks
                .Where(f => f.ReviewerId == reviewerId)
                .OrderByDescending(f => f.CreatedDate)
                .ToListAsync();
        }

        public async Task<bool> SubmitFeedbackAsync(int feedbackId)
        {
            _logger.LogInformation("Submitting feedback with ID: {FeedbackId}", feedbackId);
            var feedback = await _context.PerformanceFeedbacks.FindAsync(feedbackId);
            if (feedback == null) return false;

            feedback.Status = FeedbackStatus.Submitted;
            feedback.SubmittedDate = DateTime.UtcNow;
            feedback.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Online Exam

        public async Task<OnlineExam> CreateOnlineExamAsync(OnlineExam exam)
        {
            _logger.LogInformation("Creating online exam for review ID: {ReviewId}", exam.PerformanceReviewId);
            exam.CreatedDate = DateTime.UtcNow;
            exam.Status = ExamStatus.Draft;
            _context.OnlineExams.Add(exam);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Online exam created with ID: {Id}", exam.Id);
            return exam;
        }

        public async Task<OnlineExam> GetOnlineExamByIdAsync(int id)
        {
            _logger.LogInformation("Fetching online exam with ID: {Id}", id);
            return await _context.OnlineExams.FindAsync(id);
        }

        public async Task<IEnumerable<OnlineExam>> GetAllOnlineExamsAsync()
        {
            _logger.LogInformation("Fetching all online exams");
            return await _context.OnlineExams
                .OrderByDescending(e => e.CreatedDate)
                .ToListAsync();
        }

        public async Task<OnlineExam> UpdateOnlineExamAsync(int id, OnlineExam exam)
        {
            _logger.LogInformation("Updating online exam with ID: {Id}", id);
            var existing = await _context.OnlineExams.FindAsync(id);
            if (existing == null) return null;

            existing.Title = exam.Title;
            existing.Description = exam.Description;
            existing.ExamType = exam.ExamType;
            existing.DurationMinutes = exam.DurationMinutes;
            existing.PassingScore = exam.PassingScore;
            existing.StartDate = exam.StartDate;
            existing.EndDate = exam.EndDate;
            existing.Instructions = exam.Instructions;
            existing.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteOnlineExamAsync(int id)
        {
            _logger.LogInformation("Deleting online exam with ID: {Id}", id);
            var exam = await _context.OnlineExams.FindAsync(id);
            if (exam == null) return false;

            _context.OnlineExams.Remove(exam);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<OnlineExam>> GetExamsByEmployeeAsync(int employeeId)
        {
            _logger.LogInformation("Fetching exams for employee ID: {EmployeeId}", employeeId);
            return await _context.OnlineExams
                .Where(e => e.EmployeeId == employeeId)
                .OrderByDescending(e => e.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<OnlineExam>> GetExamsByReviewAsync(int reviewId)
        {
            _logger.LogInformation("Fetching exams for review ID: {ReviewId}", reviewId);
            return await _context.OnlineExams
                .Where(e => e.PerformanceReviewId == reviewId)
                .OrderByDescending(e => e.CreatedDate)
                .ToListAsync();
        }

        public async Task<bool> StartExamAttemptAsync(int examId, int employeeId)
        {
            _logger.LogInformation("Starting exam attempt for exam ID: {ExamId}, employee ID: {EmployeeId}", examId, employeeId);
            var exam = await _context.OnlineExams.FindAsync(examId);
            if (exam == null) return false;

            var attempt = new ExamAttempt
            {
                OnlineExamId = examId,
                EmployeeId = employeeId,
                StartTime = DateTime.UtcNow,
                Status = AttemptStatus.InProgress
            };

            _context.ExamAttempts.Add(attempt);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ExamAttempt> SubmitExamAnswersAsync(int attemptId, Dictionary<int, string> answers)
        {
            _logger.LogInformation("Submitting exam answers for attempt ID: {AttemptId}", attemptId);
            var attempt = await _context.ExamAttempts.FindAsync(attemptId);
            if (attempt == null) return null;

            attempt.EndTime = DateTime.UtcNow;
            attempt.Status = AttemptStatus.Submitted;

            foreach (var answer in answers)
            {
                var examAnswer = new ExamAnswer
                {
                    ExamAttemptId = attemptId,
                    QuestionId = answer.Key,
                    Answer = answer.Value
                };
                _context.ExamAnswers.Add(examAnswer);
            }

            await _context.SaveChangesAsync();
            return attempt;
        }

        public async Task<ExamAttempt> EvaluateExamAttemptAsync(int attemptId)
        {
            _logger.LogInformation("Evaluating exam attempt ID: {AttemptId}", attemptId);
            var attempt = await _context.ExamAttempts
                .Include(a => a.Answers)
                .FirstOrDefaultAsync(a => a.Id == attemptId);
            if (attempt == null) return null;

            var exam = await _context.OnlineExams
                .Include(e => e.Questions)
                .FirstOrDefaultAsync(e => e.Id == attempt.OnlineExamId);

            if (exam == null) return null;

            decimal totalScore = 0;
            decimal maxScore = exam.Questions.Sum(q => q.Marks);

            foreach (var answer in attempt.Answers)
            {
                var question = exam.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);
                if (question != null && question.CorrectAnswer == answer.Answer)
                {
                    answer.IsCorrect = true;
                    answer.MarksObtained = question.Marks;
                    totalScore += question.Marks;
                }
                else
                {
                    answer.IsCorrect = false;
                    answer.MarksObtained = 0;
                }
            }

            attempt.Score = totalScore;
            attempt.Percentage = maxScore > 0 ? Math.Round((totalScore / maxScore) * 100, 2) : 0;
            attempt.Status = AttemptStatus.Evaluated;
            attempt.IsPassed = attempt.Percentage >= exam.PassingScore;

            await _context.SaveChangesAsync();
            return attempt;
        }

        #endregion

        #region Performance Analytics

        public async Task<object> GetPerformanceAnalyticsAsync(int? employeeId = null, int? year = null)
        {
            _logger.LogInformation("Fetching performance analytics for employee: {EmployeeId}, year: {Year}", employeeId, year);
            var targetYear = year ?? DateTime.UtcNow.Year;

            var reviews = _context.PerformanceReviews.AsQueryable();
            if (employeeId.HasValue)
                reviews = reviews.Where(r => r.EmployeeId == employeeId.Value);
            if (year.HasValue)
                reviews = reviews.Where(r => r.CreatedDate.Year == targetYear);

            var reviewList = await reviews.ToListAsync();

            var goals = _context.PerformanceGoals.AsQueryable();
            if (employeeId.HasValue)
                goals = goals.Where(g => g.EmployeeId == employeeId.Value);

            var goalList = await goals.ToListAsync();

            return new
            {
                Year = targetYear,
                TotalReviews = reviewList.Count,
                CompletedReviews = reviewList.Count(r => r.Status == ReviewStatus.Completed),
                InProgressReviews = reviewList.Count(r => r.Status == ReviewStatus.InProgress),
                AverageRating = reviewList.Any(r => r.OverallRating > 0)
                    ? Math.Round(reviewList.Where(r => r.OverallRating > 0).Average(r => (decimal)r.OverallRating), 2)
                    : 0,
                TotalGoals = goalList.Count,
                CompletedGoals = goalList.Count(g => g.Status == GoalStatus.Completed),
                InProgressGoals = goalList.Count(g => g.Status == GoalStatus.InProgress),
                GoalCompletionRate = goalList.Count > 0
                    ? Math.Round((decimal)goalList.Count(g => g.Status == GoalStatus.Completed) / goalList.Count * 100, 2)
                    : 0,
                RatingDistribution = reviewList
                    .Where(r => r.OverallRating > 0)
                    .GroupBy(r => r.OverallRating)
                    .Select(g => new { Rating = g.Key, Count = g.Count() })
                    .ToList()
            };
        }

        public async Task<object> GetGoalCompletionRateAsync(int? departmentId = null, int? year = null)
        {
            _logger.LogInformation("Fetching goal completion rate for department: {DepartmentId}", departmentId);
            var targetYear = year ?? DateTime.UtcNow.Year;

            var goals = await _context.PerformanceGoals
                .Where(g => g.CreatedDate.Year == targetYear)
                .ToListAsync();

            return new
            {
                Year = targetYear,
                TotalGoals = goals.Count,
                CompletedGoals = goals.Count(g => g.Status == GoalStatus.Completed),
                InProgressGoals = goals.Count(g => g.Status == GoalStatus.InProgress),
                NotStartedGoals = goals.Count(g => g.Status == GoalStatus.NotStarted),
                OverdueGoals = goals.Count(g => g.Status != GoalStatus.Completed && g.TargetDate < DateTime.UtcNow),
                CompletionRate = goals.Count > 0
                    ? Math.Round((decimal)goals.Count(g => g.Status == GoalStatus.Completed) / goals.Count * 100, 2)
                    : 0,
                AverageProgress = goals.Count > 0
                    ? Math.Round(goals.Average(g => g.Progress), 2)
                    : 0
            };
        }

        public async Task<object> GetFeedbackDistributionAsync(int reviewId)
        {
            _logger.LogInformation("Fetching feedback distribution for review ID: {ReviewId}", reviewId);
            var feedbacks = await _context.PerformanceFeedbacks
                .Where(f => f.PerformanceReviewId == reviewId)
                .ToListAsync();

            return new
            {
                ReviewId = reviewId,
                TotalFeedbacks = feedbacks.Count,
                SubmittedFeedbacks = feedbacks.Count(f => f.Status == FeedbackStatus.Submitted),
                PendingFeedbacks = feedbacks.Count(f => f.Status == FeedbackStatus.Draft),
                AverageRating = feedbacks.Any(f => f.Rating > 0)
                    ? Math.Round(feedbacks.Where(f => f.Rating > 0).Average(f => (decimal)f.Rating), 2)
                    : 0,
                RatingDistribution = feedbacks
                    .Where(f => f.Rating > 0)
                    .GroupBy(f => f.Rating)
                    .Select(g => new { Rating = g.Key, Count = g.Count() })
                    .ToList(),
                ByReviewerType = feedbacks
                    .GroupBy(f => f.ReviewerType)
                    .Select(g => new { ReviewerType = g.Key.ToString(), Count = g.Count(), AvgRating = g.Any(f => f.Rating > 0) ? Math.Round(g.Where(f => f.Rating > 0).Average(f => (decimal)f.Rating), 2) : 0 })
                    .ToList()
            };
        }

        public async Task<object> GetCompetencyGapAnalysisAsync(int employeeId)
        {
            _logger.LogInformation("Fetching competency gap analysis for employee ID: {EmployeeId}", employeeId);
            var feedbacks = await _context.PerformanceFeedbacks
                .Where(f => f.EmployeeId == employeeId && f.Status == FeedbackStatus.Submitted)
                .ToListAsync();

            var goals = await _context.PerformanceGoals
                .Where(g => g.EmployeeId == employeeId)
                .ToListAsync();

            return new
            {
                EmployeeId = employeeId,
                AverageFeedbackRating = feedbacks.Any(f => f.Rating > 0)
                    ? Math.Round(feedbacks.Where(f => f.Rating > 0).Average(f => (decimal)f.Rating), 2)
                    : 0,
                GoalCompletionRate = goals.Count > 0
                    ? Math.Round((decimal)goals.Count(g => g.Status == GoalStatus.Completed) / goals.Count * 100, 2)
                    : 0,
                Strengths = feedbacks.Where(f => !string.IsNullOrEmpty(f.Strengths)).Select(f => f.Strengths).ToList(),
                AreasForImprovement = feedbacks.Where(f => !string.IsNullOrEmpty(f.AreasForImprovement)).Select(f => f.AreasForImprovement).ToList(),
                OverdueGoals = goals.Count(g => g.Status != GoalStatus.Completed && g.TargetDate < DateTime.UtcNow),
                InProgressGoals = goals.Count(g => g.Status == GoalStatus.InProgress)
            };
        }

        public async Task<object> GetPerformanceTrendAsync(int employeeId, int years = 3)
        {
            _logger.LogInformation("Fetching performance trend for employee ID: {EmployeeId} over {Years} years", employeeId, years);
            var startDate = DateTime.UtcNow.AddYears(-years);

            var reviews = await _context.PerformanceReviews
                .Where(r => r.EmployeeId == employeeId && r.CreatedDate >= startDate)
                .OrderBy(r => r.CreatedDate)
                .ToListAsync();

            return new
            {
                EmployeeId = employeeId,
                Period = $"{years} years",
                Reviews = reviews.Select(r => new
                {
                    r.Id,
                    r.ReviewPeriodStart,
                    r.ReviewPeriodEnd,
                    r.OverallRating,
                    r.Status,
                    r.ReviewType
                }).ToList(),
                RatingTrend = reviews.Any(r => r.OverallRating > 0)
                    ? reviews.Where(r => r.OverallRating > 0).Select(r => new { Date = r.CreatedDate, Rating = r.OverallRating }).ToList()
                    : new List<object>()
            };
        }

        #endregion

        #region Performance Calibration

        public async Task<bool> CalibratePerformanceRatingsAsync(int reviewId)
        {
            _logger.LogInformation("Calibrating performance ratings for review ID: {ReviewId}", reviewId);
            var feedbacks = await _context.PerformanceFeedbacks
                .Where(f => f.PerformanceReviewId == reviewId && f.Status == FeedbackStatus.Submitted)
                .ToListAsync();

            if (!feedbacks.Any()) return false;

            var avgRating = feedbacks.Where(f => f.Rating > 0).Average(f => (decimal)f.Rating);
            var review = await _context.PerformanceReviews.FindAsync(reviewId);
            if (review == null) return false;

            review.OverallRating = Math.Round(avgRating, 1);
            review.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<object> GetPerformanceDistributionAsync(int reviewId)
        {
            _logger.LogInformation("Fetching performance distribution for review ID: {ReviewId}", reviewId);
            var feedbacks = await _context.PerformanceFeedbacks
                .Where(f => f.PerformanceReviewId == reviewId && f.Status == FeedbackStatus.Submitted)
                .ToListAsync();

            return new
            {
                ReviewId = reviewId,
                Distribution = feedbacks
                    .Where(f => f.Rating > 0)
                    .GroupBy(f => f.Rating)
                    .Select(g => new { Rating = g.Key, Count = g.Count(), Percentage = feedbacks.Count > 0 ? Math.Round((decimal)g.Count() / feedbacks.Count * 100, 2) : 0 })
                    .OrderBy(d => d.Rating)
                    .ToList()
            };
        }

        public async Task<bool> ApplyForcedRankingAsync(int reviewId, string distributionType)
        {
            _logger.LogInformation("Applying forced ranking for review ID: {ReviewId} with distribution: {DistributionType}", reviewId, distributionType);
            var review = await _context.PerformanceReviews.FindAsync(reviewId);
            if (review == null) return false;

            review.CalibrationNotes = $"Forced ranking applied: {distributionType}";
            review.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Development Planning

        public async Task<object> CreateDevelopmentPlanAsync(int employeeId, int reviewId, string planDetails)
        {
            _logger.LogInformation("Creating development plan for employee ID: {EmployeeId}, review ID: {ReviewId}", employeeId, reviewId);
            var review = await _context.PerformanceReviews.FindAsync(reviewId);
            if (review == null) return null;

            review.DevelopmentPlan = planDetails;
            review.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return new { EmployeeId = employeeId, ReviewId = reviewId, PlanDetails = planDetails, CreatedAt = DateTime.UtcNow };
        }

        public async Task<object> GetDevelopmentPlanAsync(int employeeId)
        {
            _logger.LogInformation("Fetching development plan for employee ID: {EmployeeId}", employeeId);
            var review = await _context.PerformanceReviews
                .Where(r => r.EmployeeId == employeeId && !string.IsNullOrEmpty(r.DevelopmentPlan))
                .OrderByDescending(r => r.CreatedDate)
                .FirstOrDefaultAsync();

            if (review == null) return null;

            return new
            {
                EmployeeId = employeeId,
                ReviewId = review.Id,
                PlanDetails = review.DevelopmentPlan,
                OverallRating = review.OverallRating,
                Strengths = review.Strengths,
                AreasForImprovement = review.AreasForImprovement
            };
        }

        public async Task<bool> UpdateDevelopmentPlanProgressAsync(int planId, decimal progress, string comments)
        {
            _logger.LogInformation("Updating development plan progress for plan ID: {PlanId}", planId);
            var review = await _context.PerformanceReviews.FindAsync(planId);
            if (review == null) return false;

            review.CalibrationNotes = $"Progress: {progress}% - {comments}";
            review.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion
    }
}
