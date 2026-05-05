using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR_Management_System.Models.Recruitment
{
    [Table("JobPositions")]
    public class JobPosition
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string PositionCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string PositionTitle { get; set; } = string.Empty;

        [Required]
        public int DepartmentId { get; set; }

        [Required]
        public PositionType PositionType { get; set; } = PositionType.Fixed;

        [Required]
        public int NumberOfVacancies { get; set; } = 1;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MinSalary { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MaxSalary { get; set; }

        [Required]
        public JobStatus Status { get; set; } = JobStatus.Open;

        [StringLength(2000)]
        public string JobDescription { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Requirements { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Responsibilities { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Benefits { get; set; } = string.Empty;

        public DateTime? ApplicationDeadline { get; set; }

        public DateTime? ExpectedStartDate { get; set; }

        public int? ExperienceRequired { get; set; } // In years

        [StringLength(100)]
        public string EducationRequired { get; set; } = string.Empty;

        public bool IsPublished { get; set; } = false;

        public DateTime? PublishedDate { get; set; }

        // Approval workflow
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }

        [StringLength(500)]
        public string ApprovalRemarks { get; set; } = string.Empty;

        // Audit
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    public enum PositionType
    {
        Fixed = 1,
        Contract = 2,
        Temporary = 3,
        PartTime = 4,
        Internship = 5
    }

    public enum JobStatus
    {
        Draft = 1,
        Open = 2,
        OnHold = 3,
        Closed = 4,
        Cancelled = 5
    }
}