using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR_Management_System.Models.Performance
{
    [Table("PerformanceGoals")]
    public class PerformanceGoal
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int ReviewPeriodId { get; set; }

        [Required]
        [StringLength(200)]
        public string GoalTitle { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string GoalDescription { get; set; } = string.Empty;

        [Required]
        public GoalType GoalType { get; set; } = GoalType.Individual;

        [Required]
        public GoalCategory Category { get; set; } = GoalCategory.Business;

        // Metrics
        [StringLength(200)]
        public string? KeyPerformanceIndicator { get; set; }

        [StringLength(200)]
        public string? MeasurementUnit { get; set; } // Percentage, Number, Currency, etc.

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TargetValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CurrentValue { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? AchievementPercentage { get; set; }

        // Timeline
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime TargetCompletionDate { get; set; }

        public DateTime? ActualCompletionDate { get; set; }

        // Weight & Priority
        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal Weightage { get; set; } = 100; // Percentage weight in overall review

        [Required]
        public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;

        // Status
        [Required]
        public GoalStatus Status { get; set; } = GoalStatus.NotStarted;

        [Column(TypeName = "decimal(5,2)")]
        public decimal? ProgressPercentage { get; set; }

        [StringLength(500)]
        public string? StatusRemarks { get; set; }

        // Review
        [Column(TypeName = "decimal(5,2)")]
        public decimal? SelfRating { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? ManagerRating { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? FinalRating { get; set; }

        [StringLength(2000)]
        public string? SelfComments { get; set; }

        [StringLength(2000)]
        public string? ManagerComments { get; set; }

        // Dependencies
        public int? ParentGoalId { get; set; }

        [StringLength(500)]
        public string? Dependencies { get; set; }

        [StringLength(500)]
        public string? ResourcesRequired { get; set; }

        // Approval
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }

        [StringLength(500)]
        public string? ApprovalRemarks { get; set; }

        // Audit
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }

        // Navigation
        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }

        [ForeignKey("ParentGoalId")]
        public virtual PerformanceGoal? ParentGoal { get; set; }
    }

    public enum GoalType
    {
        Individual = 1,
        Team = 2,
        Department = 3,
        Company = 4
    }

    public enum GoalCategory
    {
        Business = 1,
        Operational = 2,
        Developmental = 3,
        Behavioral = 4,
        Innovation = 5,
        Customer = 6
    }

    public enum GoalStatus
    {
        NotStarted = 1,
        InProgress = 2,
        OnHold = 3,
        Completed = 4,
        Cancelled = 5,
        Overdue = 6
    }

    public enum PriorityLevel
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }
}