using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR_Management_System.Models
{
    [Table("TaskExtensionRequests")]
    public class TaskExtensionRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TaskId { get; set; }

        [ForeignKey("TaskId")]
        public virtual ProjectTask? Task { get; set; }

        [Required]
        public int RequestedByEmployeeId { get; set; }

        [ForeignKey("RequestedByEmployeeId")]
        public virtual Employee? RequestedByEmployee { get; set; }

        [Required]
        public DateTime RequestedDeadline { get; set; }

        [Required]
        [StringLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public ExtensionRequestStatus Status { get; set; } = ExtensionRequestStatus.Pending;

        public int? ReviewedByEmployeeId { get; set; }

        [ForeignKey("ReviewedByEmployeeId")]
        public virtual Employee? ReviewedByEmployee { get; set; }

        public DateTime? ReviewedAt { get; set; }

        [StringLength(500)]
        public string? ReviewComments { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Negative marking impact
        [Column(TypeName = "decimal(5,2)")]
        public decimal PerformanceDeduction { get; set; } = 0.0m;

        // Whether deduction has been applied to performance
        public bool IsDeductionApplied { get; set; } = false;

        public DateTime? DeductionAppliedAt { get; set; }
    }
}

namespace HR_Management_System.Models
{
    public enum ExtensionRequestStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        Cancelled = 4
    }
}