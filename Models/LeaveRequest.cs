using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HR_Management_System.Models.Enums;

namespace HR_Management_System.Models
{
    [Table("LeaveRequests")]
    public class LeaveRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public LeaveTypeEnum LeaveType { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(8,2)")]
        public decimal NumberOfDays { get; set; }

        [StringLength(1000)]
        public string? Reason { get; set; }

        [Required]
        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

        // Approval chain
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }

        [StringLength(500)]
        public string? ApprovalRemarks { get; set; }

        public int? RejectedBy { get; set; }
        public DateTime? RejectedDate { get; set; }

        [StringLength(500)]
        public string? RejectionReason { get; set; }

        // Supporting documents
        [StringLength(500)]
        public string? DocumentPath { get; set; }

        // Encashment
        public bool IsEncashment { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EncashmentAmount { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }
    }
}
