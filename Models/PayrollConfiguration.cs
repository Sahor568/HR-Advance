using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR_Management_System.Models
{
    public class PayrollConfiguration
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string ConfigurationName { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        public ConfigurationType Type { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        [DataType(DataType.Date)]
        public DateTime EffectiveFrom { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EffectiveTo { get; set; }

        [Required]
        public string ConfigurationJson { get; set; }

        [MaxLength(100)]
        public string ApplicableDepartment { get; set; }

        [MaxLength(100)]
        public string ApplicableEmploymentType { get; set; }

        [MaxLength(100)]
        public string ApplicableGrade { get; set; }

        public decimal MinimumSalary { get; set; }
        public decimal MaximumSalary { get; set; }

        public bool IsDefault { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        [MaxLength(450)]
        public string CreatedBy { get; set; }

        [MaxLength(450)]
        public string UpdatedBy { get; set; }

        // Navigation properties
        public virtual ICollection<PayrollComponent> Components { get; set; }
    }

    public class PayrollComponent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PayrollConfigurationId { get; set; }

        [ForeignKey("PayrollConfigurationId")]
        public virtual PayrollConfiguration PayrollConfiguration { get; set; }

        [Required]
        [MaxLength(100)]
        public string ComponentName { get; set; }

        [Required]
        public ComponentType Type { get; set; }

        [Required]
        public CalculationMethod CalculationMethod { get; set; }

        [Required]
        public decimal Value { get; set; }

        [MaxLength(50)]
        public string ValueType { get; set; } // Percentage, Fixed, Formula

        [MaxLength(500)]
        public string CalculationFormula { get; set; }

        [Required]
        public bool IsTaxable { get; set; }

        [Required]
        public bool IsSSFApplicable { get; set; }

        [Required]
        public bool IsPFApplicable { get; set; }

        [Required]
        public bool IsMandatory { get; set; } = true;

        public int DisplayOrder { get; set; }

        [MaxLength(500)]
        public string Conditions { get; set; }

        public decimal MinimumAmount { get; set; }
        public decimal MaximumAmount { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }

    public class TaxSlab
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string SlabName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EffectiveFrom { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EffectiveTo { get; set; }

        [Required]
        public decimal LowerLimit { get; set; }

        [Required]
        public decimal UpperLimit { get; set; }

        [Required]
        public decimal TaxRate { get; set; }

        public decimal FixedAmount { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class SSFConfiguration
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EffectiveFrom { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EffectiveTo { get; set; }

        [Required]
        public decimal EmployeeContributionRate { get; set; } = 10.0m; // 10% for Nepal

        [Required]
        public decimal EmployerContributionRate { get; set; } = 21.0m; // 21% for Nepal

        [Required]
        public decimal MinimumSalaryForContribution { get; set; }

        [Required]
        public decimal MaximumSalaryForContribution { get; set; }

        [MaxLength(500)]
        public string Remarks { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public enum ConfigurationType
    {
        SalaryStructure,
        Allowance,
        Deduction,
        Bonus,
        Overtime,
        Tax,
        SSF,
        PF,
        Loan,
        Advance,
        Other
    }

    public enum ComponentType
    {
        BasicSalary,
        HouseRentAllowance,
        MedicalAllowance,
        TravelAllowance,
        CommunicationAllowance,
        SpecialAllowance,
        Overtime,
        Bonus,
        Incentive,
        Arrears,
        ProfessionalTax,
        IncomeTax,
        SSFEmployee,
        SSFEmployer,
        PFEmployee,
        PFEmployer,
        LoanDeduction,
        AdvanceDeduction,
        OtherDeduction,
        OtherEarning
    }

    public enum CalculationMethod
    {
        FixedAmount,
        PercentageOfBasic,
        PercentageOfGross,
        FormulaBased,
        AttendanceBased,
        PerformanceBased,
        ManualEntry
    }
}