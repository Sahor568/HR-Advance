using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR_Management_System.Models
{
    public class Reimbursement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }

        [Required]
        [MaxLength(100)]
        public string ReimbursementType { get; set; }

        [Required]
        [MaxLength(200)]
        public string Description { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ExpenseDate { get; set; }

        [Required]
        public decimal ClaimAmount { get; set; }

        public decimal ApprovedAmount { get; set; }

        public decimal PaidAmount { get; set; }

        [MaxLength(50)]
        public string Currency { get; set; } = "NPR";

        [MaxLength(200)]
        public string ReceiptPath { get; set; }

        [MaxLength(500)]
        public string SupportingDocuments { get; set; }

        public ReimbursementStatus Status { get; set; } = ReimbursementStatus.Draft;

        [MaxLength(500)]
        public string Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? PaidAt { get; set; }

        [MaxLength(450)]
        public string ApprovedBy { get; set; }

        [MaxLength(450)]
        public string RejectedBy { get; set; }

        [MaxLength(500)]
        public string RejectionReason { get; set; }

        [MaxLength(450)]
        public string PaidBy { get; set; }

        [MaxLength(100)]
        public string PaymentMethod { get; set; }

        [MaxLength(100)]
        public string TransactionReference { get; set; }

        public int? TravelRequestId { get; set; }

        [ForeignKey("TravelRequestId")]
        public virtual TravelRequest TravelRequest { get; set; }

        // Navigation properties
        public virtual ICollection<ReimbursementApproval> Approvals { get; set; }
        public virtual ICollection<ReimbursementItem> Items { get; set; }
    }

    public class ReimbursementItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReimbursementId { get; set; }

        [ForeignKey("ReimbursementId")]
        public virtual Reimbursement Reimbursement { get; set; }

        [Required]
        [MaxLength(100)]
        public string ItemType { get; set; }

        [Required]
        [MaxLength(200)]
        public string Description { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ExpenseDate { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [MaxLength(200)]
        public string ReceiptPath { get; set; }

        [MaxLength(500)]
        public string Remarks { get; set; }

        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
    }

    public class ReimbursementApproval
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReimbursementId { get; set; }

        [ForeignKey("ReimbursementId")]
        public virtual Reimbursement Reimbursement { get; set; }

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

    public enum ReimbursementStatus
    {
        Draft,
        Submitted,
        LineManagerApproved,
        HRApproved,
        FinanceApproved,
        Approved,
        Rejected,
        Paid,
        Cancelled
    }
}