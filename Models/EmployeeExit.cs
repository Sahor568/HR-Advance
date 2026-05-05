using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR_Management_System.Models
{
    public class EmployeeExit
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }

        [Required]
        public ExitType Type { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ResignationDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime LastWorkingDate { get; set; }

        [Required]
        public int NoticePeriodDays { get; set; }

        [MaxLength(1000)]
        public string ReasonForLeaving { get; set; }

        [MaxLength(1000)]
        public string Feedback { get; set; }

        public ExitStatus Status { get; set; } = ExitStatus.ResignationSubmitted;

        [MaxLength(500)]
        public string Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? ExitCompletedAt { get; set; }

        [MaxLength(450)]
        public string ApprovedBy { get; set; }

        [MaxLength(450)]
        public string RejectedBy { get; set; }

        [MaxLength(500)]
        public string RejectionReason { get; set; }

        public bool IsNoticePeriodWaived { get; set; } = false;
        public int NoticePeriodWaivedDays { get; set; }

        public decimal NoticePeriodSalary { get; set; }
        public decimal LeaveEncashmentAmount { get; set; }
        public decimal GratuityAmount { get; set; }
        public decimal OtherDues { get; set; }
        public decimal TotalFinalSettlement { get; set; }

        public bool IsFullAndFinalCompleted { get; set; } = false;
        public DateTime? FullAndFinalCompletedAt { get; set; }

        [MaxLength(450)]
        public string FullAndFinalCompletedBy { get; set; }

        // Navigation properties
        public virtual ICollection<ExitClearance> Clearances { get; set; }
        public virtual ExitSurvey Survey { get; set; }
        public virtual ICollection<ExitDocument> Documents { get; set; }
    }

    public class ExitClearance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeExitId { get; set; }

        [ForeignKey("EmployeeExitId")]
        public virtual EmployeeExit EmployeeExit { get; set; }

        [Required]
        [MaxLength(100)]
        public string Department { get; set; }

        [Required]
        [MaxLength(200)]
        public string ClearanceItem { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public ClearanceStatus Status { get; set; } = ClearanceStatus.Pending;

        [MaxLength(450)]
        public string ClearedBy { get; set; }

        public DateTime? ClearedAt { get; set; }

        [MaxLength(500)]
        public string Remarks { get; set; }

        public bool IsMandatory { get; set; } = true;

        public int DisplayOrder { get; set; }
    }

    public class ExitSurvey
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeExitId { get; set; }

        [ForeignKey("EmployeeExitId")]
        public virtual EmployeeExit EmployeeExit { get; set; }

        [Range(1, 5)]
        public int JobSatisfaction { get; set; }

        [Range(1, 5)]
        public int ManagementSupport { get; set; }

        [Range(1, 5)]
        public int WorkEnvironment { get; set; }

        [Range(1, 5)]
        public int CareerGrowth { get; set; }

        [Range(1, 5)]
        public int CompensationBenefits { get; set; }

        [Range(1, 5)]
        public int WorkLifeBalance { get; set; }

        [MaxLength(1000)]
        public string WhatLiked { get; set; }

        [MaxLength(1000)]
        public string ImprovementsSuggested { get; set; }

        [MaxLength(1000)]
        public string ReasonForLeavingDetailed { get; set; }

        public bool WouldRecommendCompany { get; set; }

        [MaxLength(500)]
        public string NewEmployer { get; set; }

        [MaxLength(100)]
        public string NewPosition { get; set; }

        public decimal? NewSalary { get; set; }

        public DateTime SurveyCompletedAt { get; set; } = DateTime.Now;

        public bool IsAnonymous { get; set; } = false;
    }

    public class ExitDocument
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeExitId { get; set; }

        [ForeignKey("EmployeeExitId")]
        public virtual EmployeeExit EmployeeExit { get; set; }

        [Required]
        [MaxLength(100)]
        public string DocumentType { get; set; }

        [Required]
        [MaxLength(200)]
        public string DocumentName { get; set; }

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.Now;

        [MaxLength(450)]
        public string UploadedBy { get; set; }

        public bool IsMandatory { get; set; } = true;
        public bool IsSubmitted { get; set; } = true;
    }

    public enum ExitType
    {
        Resignation,
        Termination,
        Retirement,
        EndOfContract,
        Layoff,
        Death,
        Absconding,
        Other
    }

    public enum ExitStatus
    {
        ResignationSubmitted,
        UnderReview,
        Approved,
        Rejected,
        NoticePeriod,
        ClearanceInProgress,
        ClearanceCompleted,
        FullAndFinalPending,
        FullAndFinalCompleted,
        ExitCompleted
    }

    public enum ClearanceStatus
    {
        Pending,
        InProgress,
        Completed,
        Waived,
        NotApplicable
    }
}