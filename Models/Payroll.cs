using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HR_Management_System.Models.Enums;

namespace HR_Management_System.Models
{
    [Table("Payrolls")]
    public class Payroll
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int Month { get; set; }

        [Required]
        public int Year { get; set; }

        // Earnings
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Base_Salary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Grade_Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OvertimeAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal FestivalAllowance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OtherAllowances { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GrossEarnings => Base_Salary + Grade_Amount + OvertimeAmount + FestivalAllowance + OtherAllowances;

        // Deductions
        [Column(TypeName = "decimal(18,2)")]
        public decimal SSF_Employee_Contribution { get; set; } // 10% employee

        [Column(TypeName = "decimal(18,2)")]
        public decimal SSF_Employer_Contribution { get; set; } // 21% employer

        [Column(TypeName = "decimal(18,2)")]
        public decimal Tax_Deduction { get; set; } // TDS

        [Column(TypeName = "decimal(18,2)")]
        public decimal OtherDeductions { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDeductions => SSF_Employee_Contribution + Tax_Deduction + OtherDeductions;

        // Net Pay
        [Column(TypeName = "decimal(18,2)")]
        public decimal Net_Pay => GrossEarnings - TotalDeductions;

        // Overtime Details
        [Column(TypeName = "decimal(5,2)")]
        public decimal OvertimeHours { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OvertimeRate => HourlyRate * 1.5m; // 1.5x

        // Working Days
        [Column(TypeName = "decimal(5,2)")]
        public decimal WorkingDays { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal DaysWorked { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal LeaveDays { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal AbsentDays { get; set; }

        // Status
        public PayrollStatus Status { get; set; } = PayrollStatus.Draft;

        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }

        // Tax Breakdown
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxableIncome { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AnnualTaxableIncome { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxRate { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }
    }
}
