using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HR_Management_System.Models.Enums;

namespace HR_Management_System.Models
{
    [Table("LeaveBalances")]
    public class LeaveBalance
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int Year { get; set; }

        // Home Leave: 1 day per 20 working days
        [Column(TypeName = "decimal(8,2)")]
        public decimal Home_Leave_Accrued { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal Home_Leave_Used { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal Home_Leave_Balance => Home_Leave_Accrued - Home_Leave_Used;

        // Sick Leave: 12 days fully paid per year
        [Column(TypeName = "decimal(8,2)")]
        public decimal Sick_Leave_Total { get; set; } = 12;

        [Column(TypeName = "decimal(8,2)")]
        public decimal Sick_Leave_Taken { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal Sick_Leave_Balance => Sick_Leave_Total - Sick_Leave_Taken;

        // Maternity Leave: 98 days (60 days fully paid)
        [Column(TypeName = "decimal(8,2)")]
        public decimal Maternity_Leave_Total { get; set; } = 98;

        [Column(TypeName = "decimal(8,2)")]
        public decimal Maternity_Leave_Used { get; set; }

        [StringLength(20)]
        public string? Maternity_Status { get; set; } // Eligible, InProgress, Completed, NotApplicable

        // Paternity Leave: 15 days fully paid
        [Column(TypeName = "decimal(8,2)")]
        public decimal Paternity_Leave_Total { get; set; } = 15;

        [Column(TypeName = "decimal(8,2)")]
        public decimal Paternity_Leave_Used { get; set; }

        // Mourning Leave: 13 days
        [Column(TypeName = "decimal(8,2)")]
        public decimal Mourning_Leave_Total { get; set; } = 13;

        [Column(TypeName = "decimal(8,2)")]
        public decimal Mourning_Leave_Used { get; set; }

        // Public Holidays: 13 days (14 for women)
        [Column(TypeName = "decimal(8,2)")]
        public decimal Public_Holidays_Total { get; set; } = 13;

        [Column(TypeName = "decimal(8,2)")]
        public decimal Public_Holidays_Used { get; set; }

        // Encashment tracking
        [Column(TypeName = "decimal(8,2)")]
        public decimal HomeLeaveEncashed { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal SickLeaveEncashed { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }
    }
}
