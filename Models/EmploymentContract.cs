using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HR_Management_System.Models.Enums;

namespace HR_Management_System.Models
{
    [Table("EmploymentContracts")]
    public class EmploymentContract
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        [StringLength(50)]
        public string ContractNumber { get; set; } = string.Empty;

        [Required]
        public EmploymentType EmploymentType { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        [StringLength(100)]
        public string Designation { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AgreedSalary { get; set; }

        // Contract terms
        [StringLength(2000)]
        public string? TermsAndConditions { get; set; }

        [StringLength(2000)]
        public string? JobDescription { get; set; }

        // Probation
        public bool HasProbation { get; set; } = true;

        [Column(TypeName = "decimal(5,2)")]
        public decimal ProbationMonths { get; set; } = 6;

        // Appointment Letter
        [StringLength(500)]
        public string? AppointmentLetterPath { get; set; }

        public DateTime? AppointmentLetterGeneratedDate { get; set; }

        // Status
        public ContractStatus Status { get; set; } = ContractStatus.Active;

        [StringLength(500)]
        public string? TerminationReason { get; set; }

        // Renewal
        public int? RenewedFromContractId { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }
    }
}
