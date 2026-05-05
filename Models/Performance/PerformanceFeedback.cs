using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HR_Management_System.Models.Identity;

namespace HR_Management_System.Models.Performance
{
    public class PerformanceFeedback
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PerformanceReviewId { get; set; }

        [ForeignKey("PerformanceReviewId")]
        public virtual PerformanceReview PerformanceReview { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }

        [Required]
        [MaxLength(450)]
        public string ReviewerId { get; set; }

        [ForeignKey("ReviewerId")]
        public virtual ApplicationUser Reviewer { get; set; }

        [Required]
        public ReviewerType ReviewerType { get; set; }

        [Required]
        [Range(1, 5)]
        public int OverallRating { get; set; }

        [MaxLength(1000)]
        public string Strengths { get; set; }

        [MaxLength(1000)]
        public string AreasForImprovement { get; set; }

        [MaxLength(1000)]
        public string DevelopmentPlan { get; set; }

        [MaxLength(1000)]
        public string Comments { get; set; }

        public FeedbackStatus Status { get; set; } = FeedbackStatus.Draft;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? SubmittedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<FeedbackCriteria> CriteriaRatings { get; set; }
    }

    public class FeedbackCriteria
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PerformanceFeedbackId { get; set; }

        [ForeignKey("PerformanceFeedbackId")]
        public virtual PerformanceFeedback PerformanceFeedback { get; set; }

        [Required]
        [MaxLength(100)]
        public string CriteriaName { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(500)]
        public string Comments { get; set; }

        public decimal Weightage { get; set; } = 1.0m;
        public decimal WeightedScore { get; set; }
    }

    public class OnlineExam
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PerformanceReviewId { get; set; }

        [ForeignKey("PerformanceReviewId")]
        public virtual PerformanceReview PerformanceReview { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }

        [Required]
        [MaxLength(200)]
        public string ExamTitle { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        public ExamType Type { get; set; }

        public int TotalQuestions { get; set; }
        public int TotalMarks { get; set; }
        public int PassingMarks { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime StartDateTime { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime EndDateTime { get; set; }

        public int DurationMinutes { get; set; }

        public ExamStatus Status { get; set; } = ExamStatus.Scheduled;

        public int? Score { get; set; }
        public decimal? Percentage { get; set; }
        public bool? IsPassed { get; set; }

        public DateTime? AttemptedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? EvaluatedAt { get; set; }

        [MaxLength(500)]
        public string Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<ExamQuestion> Questions { get; set; }
        public virtual ICollection<ExamAttempt> Attempts { get; set; }
    }

    public class ExamQuestion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OnlineExamId { get; set; }

        [ForeignKey("OnlineExamId")]
        public virtual OnlineExam OnlineExam { get; set; }

        [Required]
        [MaxLength(1000)]
        public string QuestionText { get; set; }

        [Required]
        public QuestionType Type { get; set; }

        [MaxLength(500)]
        public string OptionA { get; set; }

        [MaxLength(500)]
        public string OptionB { get; set; }

        [MaxLength(500)]
        public string OptionC { get; set; }

        [MaxLength(500)]
        public string OptionD { get; set; }

        [MaxLength(500)]
        public string OptionE { get; set; }

        [Required]
        [MaxLength(10)]
        public string CorrectAnswer { get; set; }

        public int Marks { get; set; } = 1;

        public int DisplayOrder { get; set; }

        [MaxLength(500)]
        public string Explanation { get; set; }
    }

    public class ExamAttempt
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OnlineExamId { get; set; }

        [ForeignKey("OnlineExamId")]
        public virtual OnlineExam OnlineExam { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime StartedAt { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? CompletedAt { get; set; }

        public int TimeTakenSeconds { get; set; }

        public int TotalQuestions { get; set; }
        public int AttemptedQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public int WrongAnswers { get; set; }
        public int Score { get; set; }
        public decimal Percentage { get; set; }
        public bool IsPassed { get; set; }

        public AttemptStatus Status { get; set; } = AttemptStatus.InProgress;

        [MaxLength(500)]
        public string Remarks { get; set; }

        // Navigation properties
        public virtual ICollection<ExamAnswer> Answers { get; set; }
    }

    public class ExamAnswer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ExamAttemptId { get; set; }

        [ForeignKey("ExamAttemptId")]
        public virtual ExamAttempt ExamAttempt { get; set; }

        [Required]
        public int ExamQuestionId { get; set; }

        [ForeignKey("ExamQuestionId")]
        public virtual ExamQuestion ExamQuestion { get; set; }

        [MaxLength(10)]
        public string SelectedAnswer { get; set; }

        public bool IsCorrect { get; set; }
        public int MarksObtained { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime AnsweredAt { get; set; }
    }

    public enum ReviewerType
    {
        Self,
        Manager,
        Peer,
        Subordinate,
        Customer,
        Other
    }

    public enum FeedbackStatus
    {
        Draft,
        Submitted,
        Reviewed,
        Finalized
    }

    public enum ExamType
    {
        Technical,
        Behavioral,
        Aptitude,
        Knowledge,
        Skills,
        Compliance,
        Other
    }

    public enum ExamStatus
    {
        Scheduled,
        InProgress,
        Completed,
        Evaluated,
        Cancelled
    }

    public enum QuestionType
    {
        MultipleChoice,
        TrueFalse,
        ShortAnswer,
        Essay,
        CaseStudy,
        Practical
    }

    public enum AttemptStatus
    {
        NotStarted,
        InProgress,
        Completed,
        Submitted,
        Evaluated,
        Timeout
    }
}