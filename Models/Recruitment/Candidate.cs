using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HR_Management_System.Models.Enums;

namespace HR_Management_System.Models.Recruitment
{
    [Table("Candidates")]
    public class Candidate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string CandidateCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? MiddleName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(20)]
        public string? AlternatePhone { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public Gender Gender { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        [StringLength(20)]
        public string? ZipCode { get; set; }

        // Application details
        [Required]
        public int JobPositionId { get; set; }

        [Required]
        public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;

        [Required]
        public ApplicationSource Source { get; set; } = ApplicationSource.OnlinePortal;

        [StringLength(500)]
        public string? ReferredBy { get; set; }

        [StringLength(2000)]
        public string? CoverLetter { get; set; }

        [StringLength(500)]
        public string? ResumeFilePath { get; set; }

        [StringLength(500)]
        public string? PortfolioUrl { get; set; }

        [StringLength(500)]
        public string? LinkedInProfile { get; set; }

        // Education & Experience
        [StringLength(100)]
        public string? HighestEducation { get; set; }

        [StringLength(100)]
        public string? University { get; set; }

        public decimal? YearsOfExperience { get; set; }

        [StringLength(200)]
        public string? CurrentCompany { get; set; }

        [StringLength(200)]
        public string? CurrentPosition { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CurrentSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ExpectedSalary { get; set; }

        [StringLength(100)]
        public string? NoticePeriod { get; set; } // e.g., "1 month", "Immediate"

        // Status & Workflow
        [Required]
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;

        public DateTime? StatusChangedDate { get; set; }

        [StringLength(500)]
        public string? StatusRemarks { get; set; }

        public int? AssignedTo { get; set; } // HR staff ID

        // Interview details
        public DateTime? FirstInterviewDate { get; set; }
        public DateTime? SecondInterviewDate { get; set; }
        public DateTime? FinalInterviewDate { get; set; }

        [StringLength(2000)]
        public string? InterviewFeedback { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? InterviewScore { get; set; }

        // Offer details
        public DateTime? OfferDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OfferedSalary { get; set; }

        public DateTime? OfferExpiryDate { get; set; }

        [StringLength(500)]
        public string? OfferRemarks { get; set; }

        public bool? OfferAccepted { get; set; }

        public DateTime? OfferAcceptedDate { get; set; }

        // Onboarding
        public DateTime? ExpectedJoiningDate { get; set; }

        public int? ConvertedToEmployeeId { get; set; } // If hired

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        [ForeignKey("JobPositionId")]
        public virtual JobPosition? JobPosition { get; set; }
    }

    public enum ApplicationSource
    {
        OnlinePortal = 1,
        Referral = 2,
        JobPortal = 3,
        SocialMedia = 4,
        CampusRecruitment = 5,
        WalkIn = 6,
        Agency = 7,
        Other = 8
    }

    public enum ApplicationStatus
    {
        Applied = 1,
        Screening = 2,
        Shortlisted = 3,
        FirstInterview = 4,
        SecondInterview = 5,
        FinalInterview = 6,
        ReferenceCheck = 7,
        BackgroundCheck = 8,
        OfferExtended = 9,
        OfferAccepted = 10,
        OfferDeclined = 11,
        Onboarding = 12,
        Hired = 13,
        Rejected = 14,
        Withdrawn = 15,
        OnHold = 16
    }
}