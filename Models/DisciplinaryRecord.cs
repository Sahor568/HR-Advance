using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HR_Management_System.Models.Enums;

namespace HR_Management_System.Models
{
    [Table("DisciplinaryRecords")]
    public class DisciplinaryRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public DisciplinaryActionType ActionType { get; set; }

        [Required]
        public DateTime IncidentDate { get; set; }

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        // Section 131 Compliance
        public bool IsSection131Compliance { get; set; }

        [StringLength(1000)]
        public string? Section131Details { get; set; }

        // Warning details
        [StringLength(2000)]
        public string? WarningDetails { get; set; }

        // Issued by
        [Required]
        public int IssuedBy { get; set; }

        [Required]
        public DateTime IssuedDate { get; set; }

        // Suspension
        public DateTime? SuspensionStartDate { get; set; }
        public DateTime? SuspensionEndDate { get; set; }

        // Resolution
        [StringLength(2000)]
        public string? Resolution { get; set; }

        public DateTime? ResolutionDate { get; set; }

        public bool IsResolved { get; set; }

        // Supporting documents
        [StringLength(500)]
        public string? DocumentPath { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }
    }
}
