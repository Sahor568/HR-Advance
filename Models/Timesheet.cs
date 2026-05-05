using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR_Management_System.Models
{
    public class Timesheet
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        [Required]
        [Range(0, 24)]
        public decimal TotalHours { get; set; }

        public decimal HoursWorked { get; set; }

        [Range(0, 24)]
        public decimal OvertimeHours { get; set; }

        [Range(0, 24)]
        public decimal BreakHours { get; set; }

        [MaxLength(500)]
        public string TaskDescription { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [MaxLength(100)]
        public string ProjectCode { get; set; }

        [MaxLength(100)]
        public string ClientCode { get; set; }

        public TimesheetStatus Status { get; set; } = TimesheetStatus.Draft;

        [MaxLength(500)]
        public string Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }

        [MaxLength(450)]
        public string ApprovedBy { get; set; }

        [MaxLength(500)]
        public string RejectionReason { get; set; }

        public bool IsBillable { get; set; } = true;
        public decimal BillableRate { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public enum TimesheetStatus
    {
        Draft,
        Submitted,
        Approved,
        Rejected,
        Paid
    }
}