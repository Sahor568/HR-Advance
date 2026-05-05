using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR_Management_System.Models
{
    public class TravelRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }

        [Required]
        [MaxLength(200)]
        public string Purpose { get; set; }

        [Required]
        public TravelType Type { get; set; }

        [Required]
        [MaxLength(100)]
        public string Destination { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DepartureDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ReturnDate { get; set; }

        public int DurationDays { get; set; }

        [MaxLength(100)]
        public string TravelMode { get; set; }

        [MaxLength(200)]
        public string AccommodationDetails { get; set; }

        public decimal EstimatedCost { get; set; }

        public decimal ApprovedBudget { get; set; }

        public decimal ActualCost { get; set; }

        [MaxLength(500)]
        public string Itinerary { get; set; }

        public TravelRequestStatus Status { get; set; } = TravelRequestStatus.Draft;

        [MaxLength(500)]
        public string Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }

        [MaxLength(450)]
        public string ApprovedBy { get; set; }

        [MaxLength(450)]
        public string RejectedBy { get; set; }

        [MaxLength(500)]
        public string RejectionReason { get; set; }

        public DateTime? TravelCompletedAt { get; set; }

        // Navigation properties
        public virtual ICollection<TravelExpense> Expenses { get; set; }
        public virtual ICollection<TravelApproval> Approvals { get; set; }
    }

    public class TravelExpense
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TravelRequestId { get; set; }

        [ForeignKey("TravelRequestId")]
        public virtual TravelRequest TravelRequest { get; set; }

        [Required]
        [MaxLength(100)]
        public string ExpenseType { get; set; }

        [Required]
        [MaxLength(200)]
        public string Description { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ExpenseDate { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [MaxLength(50)]
        public string Currency { get; set; } = "NPR";

        [MaxLength(200)]
        public string ReceiptPath { get; set; }

        public bool IsReimbursable { get; set; } = true;

        public ExpenseStatus Status { get; set; } = ExpenseStatus.Pending;

        [MaxLength(500)]
        public string Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? VerifiedAt { get; set; }
        public DateTime? ReimbursedAt { get; set; }
    }

    public class TravelApproval
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TravelRequestId { get; set; }

        [ForeignKey("TravelRequestId")]
        public virtual TravelRequest TravelRequest { get; set; }

        [Required]
        [MaxLength(450)]
        public string ApproverId { get; set; }

        [Required]
        public ApprovalLevel Level { get; set; }

        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

        [MaxLength(500)]
        public string Comments { get; set; }

        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public enum TravelType
    {
        Business,
        Training,
        Conference,
        ClientMeeting,
        SiteVisit,
        Recruitment,
        Other
    }

    public enum TravelRequestStatus
    {
        Draft,
        Submitted,
        LineManagerApproved,
        HRApproved,
        FinanceApproved,
        Approved,
        Rejected,
        Cancelled,
        Completed
    }

    public enum ExpenseStatus
    {
        Pending,
        Verified,
        Approved,
        Rejected,
        Reimbursed
    }

    public enum ApprovalLevel
    {
        LineManager,
        DepartmentHead,
        HR,
        Finance,
        CEO
    }

    public enum ApprovalStatus
    {
        Pending,
        Approved,
        Rejected,
        Deferred
    }
}