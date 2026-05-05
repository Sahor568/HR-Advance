using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HR_Management_System.Models.Enums;

namespace HR_Management_System.Models.Recruitment
{
    public class WorkforcePlanning
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string PlanName { get; set; }

        [Required]
        public PlanningType Type { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        [MaxLength(100)]
        public string Department { get; set; }

        [Required]
        public int RequiredPositions { get; set; }

        public int CurrentStrength { get; set; }
        public int Gap { get; set; }

        [MaxLength(500)]
        public string Justification { get; set; }

        [MaxLength(500)]
        public string SkillsRequired { get; set; }

        [MaxLength(500)]
        public string QualificationsRequired { get; set; }

        public decimal BudgetAllocated { get; set; }
        public decimal EstimatedCostPerHire { get; set; }
        public decimal TotalEstimatedCost { get; set; }

        public PlanningStatus Status { get; set; } = PlanningStatus.Draft;

        [MaxLength(500)]
        public string Remarks { get; set; }

        public int? FiscalYear { get; set; }
        public int? CurrentHeadcount { get; set; }
        public int? RequiredHeadcount { get; set; }
        public decimal? BudgetAllocation { get; set; }
        public DateTime? TargetDate { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }

        [MaxLength(450)]
        public string CreatedBy { get; set; }

        [MaxLength(450)]
        public string ApprovedBy { get; set; }

        // Navigation properties
        public virtual ICollection<EmployeeRequisition> Requisitions { get; set; }
    }

    public class EmployeeRequisition
    {
        [Key]
        public int Id { get; set; }

        public int? WorkforcePlanningId { get; set; }

        [ForeignKey("WorkforcePlanningId")]
        public virtual WorkforcePlanning WorkforcePlanning { get; set; }

        [Required]
        [MaxLength(100)]
        public string RequisitionNumber { get; set; }

        [Required]
        [MaxLength(100)]
        public string PositionTitle { get; set; }

        [Required]
        public int NumberOfOpenings { get; set; }

        [Required]
        [MaxLength(100)]
        public string Department { get; set; }

        [Required]
        public EmploymentType EmploymentType { get; set; }

        [Required]
        public PositionType PositionType { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime RequiredByDate { get; set; }

        [MaxLength(500)]
        public string JobDescription { get; set; }

        [MaxLength(500)]
        public string Qualifications { get; set; }

        [MaxLength(500)]
        public string ExperienceRequired { get; set; }

        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }

        [MaxLength(50)]
        public string UrgencyLevel { get; set; }
        public DateTime? TargetDate { get; set; }

        [MaxLength(500)]
        public string Benefits { get; set; }

        public RequisitionStatus Status { get; set; } = RequisitionStatus.Draft;

        [MaxLength(500)]
        public string Remarks { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }
        public bool DepartmentHeadApproval { get; set; }
        [MaxLength(450)]
        public string DepartmentHeadApprovedBy { get; set; }
        public DateTime? DepartmentHeadApprovedDate { get; set; }
        public bool HRApproval { get; set; }
        [MaxLength(450)]
        public string HRApprovedBy { get; set; }
        public DateTime? HRApprovedDate { get; set; }
        public bool ManagementApproval { get; set; }
        [MaxLength(450)]
        public string ManagementApprovedBy { get; set; }
        public DateTime? ManagementApprovedDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? ClosedAt { get; set; }

        [MaxLength(450)]
        public string RequestedBy { get; set; }

        [MaxLength(450)]
        public string ApprovedBy { get; set; }

        [MaxLength(450)]
        public string HiringManager { get; set; }

        // Navigation properties
        public virtual ICollection<JobPosition> JobPositions { get; set; }
    }

    public enum PlanningType
    {
        Annual,
        Quarterly,
        Monthly,
        AdHoc,
        Replacement,
        Expansion,
        Restructuring
    }

    public enum PlanningStatus
    {
        Draft,
        Submitted,
        Approved,
        Rejected,
        InProgress,
        Completed,
        Cancelled
    }

    public enum RequisitionStatus
    {
        Draft,
        Submitted,
        LineManagerApproved,
        HRApproved,
        FinanceApproved,
        Approved,
        Rejected,
        OnHold,
        Closed,
        Filled
    }
}