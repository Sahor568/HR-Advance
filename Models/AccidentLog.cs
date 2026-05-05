using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HR_Management_System.Models.Enums;

namespace HR_Management_System.Models
{
    [Table("AccidentLogs")]
    public class AccidentLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public DateTime AccidentDate { get; set; }

        [Required]
        public TimeSpan AccidentTime { get; set; }

        [Required]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public AccidentSeverity Severity { get; set; }

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? ImmediateActionTaken { get; set; }

        [StringLength(1000)]
        public string? RootCause { get; set; }

        [StringLength(2000)]
        public string? PreventiveMeasures { get; set; }

        // Witness
        [StringLength(200)]
        public string? WitnessNames { get; set; }

        // Injury details
        [StringLength(1000)]
        public string? InjuryDetails { get; set; }

        public bool RequiredHospitalization { get; set; }

        public int? DaysLost { get; set; }

        // Report to authorities
        public bool ReportedToAuthorities { get; set; }

        public DateTime? ReportedDate { get; set; }

        [StringLength(500)]
        public string? ReportReferenceNumber { get; set; }

        // Documents
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
