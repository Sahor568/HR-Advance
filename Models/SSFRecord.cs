using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR_Management_System.Models
{
    [Table("SSFRecords")]
    public class SSFRecord
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

        // Employee contribution: 10% of basic salary
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal EmployeeContribution { get; set; }

        // Employer contribution: 21% (includes provident fund + gratuity)
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal EmployerContribution { get; set; }

        // Breakdown of employer contribution
        [Column(TypeName = "decimal(18,2)")]
        public decimal ProvidentFundEmployer { get; set; } // Part of 21%

        [Column(TypeName = "decimal(18,2)")]
        public decimal GratuityEmployer { get; set; } // Part of 21%

        [Column(TypeName = "decimal(18,2)")]
        public decimal SSFEmployer { get; set; } // Part of 21%

        // Cumulative balance
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalEmployeeBalance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalEmployerBalance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalBalance => TotalEmployeeBalance + TotalEmployerBalance;

        // Base salary for the month
        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseSalary { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }
    }
}
