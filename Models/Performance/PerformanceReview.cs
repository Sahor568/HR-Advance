using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR_Management_System.Models.Performance
{
    [Table("PerformanceReviews")]
    public class PerformanceReview
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int ReviewPeriodId { get; set; }

        [Required]
        public ReviewType ReviewType { get; set; } = ReviewType.Annual;

        [Required]
        public DateTime ReviewStartDate { get; set; }

        [Required]
        public DateTime ReviewEndDate { get; set; }

        [Required]
        public DateTime ReviewDate { get; set; } = DateTime.UtcNow;

        // Reviewers (360-degree)
        [Required]
        public int ReviewerId { get; set; } // Primary reviewer (Manager)

        [StringLength(500)]
        public string? PeerReviewerIds { get; set; } // Comma-separated peer IDs

        [StringLength(500)]
        public string? SubordinateReviewerIds { get; set; } // Comma-separated subordinate IDs

        [StringLength(500)]
        public string? SelfReviewerId { get; set; } // Employee self-review

        // Status
        [Required]
        public ReviewStatus Status { get; set; } = ReviewStatus.Draft;

        public DateTime? SelfReviewSubmittedDate { get; set; }
        public DateTime? ManagerReviewSubmittedDate { get; set; }
        public DateTime? HRReviewSubmittedDate { get; set; }
        public DateTime? FinalizedDate { get; set; }

        // Scores
        [Column(TypeName = "decimal(5,2)")]
        public decimal? SelfReviewScore { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? ManagerReviewScore { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? PeerReviewScore { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? SubordinateReviewScore { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? FinalScore { get; set; }

        [Required]
        public RatingScale FinalRating { get; set; } = RatingScale.NotRated;

        // Feedback
        [StringLength(2000)]
        public string? SelfReviewComments { get; set; }

        [StringLength(2000)]
        public string? ManagerFeedback { get; set; }

        [StringLength(2000)]
        public string? HRFeedback { get; set; }

        [StringLength(2000)]
        public string? EmployeeComments { get; set; }

        [StringLength(2000)]
        public string? DevelopmentPlan { get; set; }

        [StringLength(2000)]
        public string? Strengths { get; set; }

        [StringLength(2000)]
        public string? AreasForImprovement { get; set; }

        [StringLength(2000)]
        public string? CareerGoals { get; set; }

        // Goals & Achievements
        [StringLength(2000)]
        public string? GoalsAchieved { get; set; }

        [StringLength(2000)]
        public string? GoalsNotAchieved { get; set; }

        [StringLength(2000)]
        public string? NextPeriodGoals { get; set; }

        // Promotion & Compensation
        public bool RecommendedForPromotion { get; set; } = false;

        [StringLength(500)]
        public string? RecommendedPosition { get; set; }

        public bool RecommendedForSalaryIncrease { get; set; } = false;

        [Column(TypeName = "decimal(5,2)")]
        public decimal? RecommendedIncreasePercentage { get; set; }

        [StringLength(1000)]
        public string? PromotionRemarks { get; set; }

        // Training needs
        [StringLength(2000)]
        public string? TrainingNeeds { get; set; }

        [StringLength(1000)]
        public string? RecommendedTrainings { get; set; }

        // Approval
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }

        [StringLength(500)]
        public string? ApprovalRemarks { get; set; }

        // Documents
        [StringLength(500)]
        public string? ReviewDocumentPath { get; set; }

        [StringLength(500)]
        public string? SignedDocumentPath { get; set; }

        // Audit
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? OverallRating { get; set; }

        public DateTime? ReviewPeriodStart { get; set; }
        public DateTime? ReviewPeriodEnd { get; set; }

        [StringLength(1000)]
        public string CalibrationNotes { get; set; }

        // Navigation
        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }

        [ForeignKey("ReviewerId")]
        public virtual Employee? Reviewer { get; set; }
    }

    public enum ReviewType
    {
        Probation = 1,
        Quarterly = 2,
        HalfYearly = 3,
        Annual = 4,
        Promotion = 5,
        Special = 6
    }

    public enum ReviewStatus
    {
        Draft = 1,
        SelfReviewPending = 2,
        ManagerReviewPending = 3,
        HRReviewPending = 4,
        DiscussionPending = 5,
        Finalized = 6,
        Approved = 7,
        Rejected = 8,
        Archived = 9
    }

    public enum RatingScale
    {
        NotRated = 0,
        Poor = 1,          // 1-2.9
        NeedsImprovement = 2, // 3-3.9
        MeetsExpectations = 3, // 4-4.4
        ExceedsExpectations = 4, // 4.5-4.9
        Outstanding = 5    // 5
    }
}