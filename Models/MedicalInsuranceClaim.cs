using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HR_Management_System.Models.Enums;

namespace HR_Management_System.Models
{
    [Table("MedicalInsuranceClaims")]
    public class MedicalInsuranceClaim
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        [StringLength(50)]
        public string ClaimNumber { get; set; } = string.Empty;

        [Required]
        public DateTime ClaimDate { get; set; }

        [Required]
        [StringLength(200)]
        public string ClaimType { get; set; } = string.Empty; // Medical, Accident

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ClaimAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ApprovedAmount { get; set; }

        // Coverage limits as per Nepal law
        [Column(TypeName = "decimal(18,2)")]
        public decimal MedicalCoverageLimit { get; set; } = 100000; // 1 lakh medical

        [Column(TypeName = "decimal(18,2)")]
        public decimal AccidentCoverageLimit { get; set; } = 700000; // 7 lakh accident

        [Required]
        public ClaimStatus Status { get; set; } = ClaimStatus.Pending;

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(2000)]
        public string? HospitalDetails { get; set; }

        public DateTime? TreatmentStartDate { get; set; }
        public DateTime? TreatmentEndDate { get; set; }

        // Approval
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }

        [StringLength(500)]
        public string? ApprovalRemarks { get; set; }

        [StringLength(500)]
        public string? RejectionReason { get; set; }

        // Documents
        [StringLength(500)]
        public string? BillDocumentPath { get; set; }

        [StringLength(500)]
        public string? PrescriptionDocumentPath { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }
    }
}
