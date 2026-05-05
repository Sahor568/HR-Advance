using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HR_Management_System.Models.Enums;

namespace HR_Management_System.Models.Recruitment
{
    [Table("OfferLetters")]
    public class OfferLetter
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string OfferNumber { get; set; } = string.Empty;

        [Required]
        public int CandidateId { get; set; }

        [Required]
        public int JobPositionId { get; set; }

        // Offer details
        [Required]
        [StringLength(200)]
        public string PositionTitle { get; set; } = string.Empty;

        [Required]
        public EmploymentType EmploymentType { get; set; }

        [Required]
        public DateTime OfferDate { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime JoiningDate { get; set; }

        [Required]
        public int ProbationPeriodMonths { get; set; } = 6;

        // Compensation
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BasicSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal HouseRentAllowance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MedicalAllowance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TravelAllowance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OtherAllowances { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal GrossSalary { get; set; }

        // Benefits
        [StringLength(1000)]
        public string? BenefitsDescription { get; set; }

        public bool IncludesPF { get; set; } = true;
        public bool IncludesGratuity { get; set; } = true;
        public bool IncludesInsurance { get; set; } = true;
        public bool IncludesBonus { get; set; } = true;

        // Terms and conditions
        [StringLength(2000)]
        public string? TermsAndConditions { get; set; }

        [StringLength(1000)]
        public string? ReportingTo { get; set; }

        [StringLength(200)]
        public string? WorkLocation { get; set; }

        [StringLength(200)]
        public string? WorkingHours { get; set; }

        // Status
        [Required]
        public OfferStatus Status { get; set; } = OfferStatus.Draft;

        public DateTime? SentDate { get; set; }
        public DateTime? AcceptedDate { get; set; }
        public DateTime? DeclinedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        [StringLength(500)]
        public string? AcceptanceRemarks { get; set; }

        [StringLength(500)]
        public string? DeclineReason { get; set; }

        // Document
        [StringLength(500)]
        public string? OfferDocumentPath { get; set; }

        [StringLength(500)]
        public string? SignedDocumentPath { get; set; }

        // Approval
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }

        [StringLength(500)]
        public string? ApprovalRemarks { get; set; }

        // Conversion to employee
        public int? ConvertedToEmployeeId { get; set; }
        public DateTime? ConversionDate { get; set; }

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

    public enum OfferStatus
    {
        Draft = 1,
        Approved = 2,
        Sent = 3,
        Accepted = 4,
        Declined = 5,
        Expired = 6,
        Withdrawn = 7,
        Converted = 8
    }
}