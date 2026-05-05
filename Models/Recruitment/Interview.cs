using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR_Management_System.Models.Recruitment
{
    [Table("Interviews")]
    public class Interview
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CandidateId { get; set; }

        [Required]
        public int JobPositionId { get; set; }

        [Required]
        public InterviewRound Round { get; set; } = InterviewRound.First;

        [Required]
        [StringLength(200)]
        public string InterviewType { get; set; } = string.Empty; // Technical, HR, Managerial, etc.

        [Required]
        public DateTime ScheduledDate { get; set; }

        [Required]
        public TimeSpan ScheduledTime { get; set; }

        public DateTime? ActualStartTime { get; set; }
        public DateTime? ActualEndTime { get; set; }

        [Required]
        [StringLength(200)]
        public string InterviewMode { get; set; } = "In-Person"; // In-Person, Video Call, Phone Call

        [StringLength(500)]
        public string? InterviewLink { get; set; }

        [StringLength(500)]
        public string? Location { get; set; }

        // Interviewers
        [Required]
        [StringLength(500)]
        public string InterviewerIds { get; set; } = string.Empty; // Comma-separated employee IDs

        [StringLength(500)]
        public string InterviewerNames { get; set; } = string.Empty; // Comma-separated names

        // Status
        [Required]
        public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;

        public DateTime? StatusChangedDate { get; set; }

        [StringLength(500)]
        public string? StatusRemarks { get; set; }

        // Feedback
        [StringLength(2000)]
        public string? TechnicalFeedback { get; set; }

        [StringLength(2000)]
        public string? CommunicationFeedback { get; set; }

        [StringLength(2000)]
        public string? CulturalFitFeedback { get; set; }

        [StringLength(2000)]
        public string? OverallFeedback { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? TechnicalScore { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? CommunicationScore { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? CulturalFitScore { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? OverallScore { get; set; }

        [Required]
        public Recommendation Recommendation { get; set; } = Recommendation.Pending;

        [StringLength(1000)]
        public string? RecommendationRemarks { get; set; }

        // Follow-up
        public DateTime? NextInterviewDate { get; set; }
        public TimeSpan? NextInterviewTime { get; set; }

        [StringLength(500)]
        public string? NextInterviewMode { get; set; }

        // Documents
        [StringLength(500)]
        public string? InterviewNotesPath { get; set; }

        [StringLength(500)]
        public string? RecordingPath { get; set; }

        // Audit
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }

        // Navigation
        [ForeignKey("CandidateId")]
        public virtual Candidate? Candidate { get; set; }

        [ForeignKey("JobPositionId")]
        public virtual JobPosition? JobPosition { get; set; }
    }

    public enum InterviewRound
    {
        First = 1,
        Second = 2,
        Third = 3,
        Final = 4,
        Additional = 5
    }

    public enum InterviewStatus
    {
        Scheduled = 1,
        InProgress = 2,
        Completed = 3,
        Cancelled = 4,
        Rescheduled = 5,
        NoShow = 6
    }

    public enum Recommendation
    {
        Pending = 0,
        StronglyRecommended = 1,
        Recommended = 2,
        NotRecommended = 3,
        OnHold = 4,
        FurtherEvaluation = 5
    }
}