using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HR_Management_System.Models.Enums;

namespace HR_Management_System.Models
{
    [Table("LaborAuditReports")]
    public class LaborAuditReport
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ReportNumber { get; set; } = string.Empty;

        [Required]
        public ReportType ReportType { get; set; }

        [Required]
        public DateTime PeriodStart { get; set; }

        [Required]
        public DateTime PeriodEnd { get; set; }

        [Required]
        public DateTime GeneratedDate { get; set; }

        public int GeneratedBy { get; set; }

        // Compliance Summary
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public int EmployeesOnProbation { get; set; }

        // Attendance Compliance
        [Column(TypeName = "decimal(5,2)")]
        public decimal AverageWorkingHours { get; set; }

        public int OvertimeViolations { get; set; }
        public int WorkHourViolations { get; set; }

        // Leave Compliance
        public int LeaveViolations { get; set; }
        public int MaternityLeaveGranted { get; set; }
        public int PaternityLeaveGranted { get; set; }

        // Payroll Compliance
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPayrollAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalTaxDeducted { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalSSFContribution { get; set; }

        public int PayrollDiscrepancies { get; set; }

        // OHS
        public int TotalAccidents { get; set; }
        public int UnreportedAccidents { get; set; }
        public int PendingInsuranceClaims { get; set; }

        // Disciplinary
        public int TotalDisciplinaryActions { get; set; }
        public int Section131Cases { get; set; }
        public int UnresolvedCases { get; set; }

        // Festival Bonus
        public int FestivalBonusPaid { get; set; }
        public int FestivalBonusPending { get; set; }

        // Report Status
        public bool IsApproved { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }

        [StringLength(2000)]
        public string? Remarks { get; set; }

        [StringLength(500)]
        public string? ReportFilePath { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
