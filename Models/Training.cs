using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR_Management_System.Models
{
    public class Training
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        public TrainingType Type { get; set; }

        [Required]
        public TrainingCategory Category { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        [MaxLength(100)]
        public string Duration { get; set; }

        [Required]
        [MaxLength(200)]
        public string Provider { get; set; }

        [MaxLength(200)]
        public string Location { get; set; }

        [MaxLength(200)]
        public string OnlineLink { get; set; }

        public decimal Cost { get; set; }

        [MaxLength(50)]
        public string Currency { get; set; } = "NPR";

        public int MaxParticipants { get; set; }

        public int CurrentParticipants { get; set; }

        public TrainingStatus Status { get; set; } = TrainingStatus.Scheduled;

        [MaxLength(500)]
        public string Prerequisites { get; set; }

        [MaxLength(500)]
        public string LearningObjectives { get; set; }

        [MaxLength(500)]
        public string MaterialsProvided { get; set; }

        [MaxLength(200)]
        public string TrainerName { get; set; }

        [MaxLength(500)]
        public string TrainerQualifications { get; set; }

        public bool IsMandatory { get; set; }

        public bool IsCertificationProvided { get; set; }

        [MaxLength(200)]
        public string CertificationName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<TrainingAttendance> Attendances { get; set; }
        public virtual ICollection<TrainingEvaluation> Evaluations { get; set; }
    }

    public class TrainingAttendance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TrainingId { get; set; }

        [ForeignKey("TrainingId")]
        public virtual Training Training { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }

        public TrainingAttendanceStatus Status { get; set; } = TrainingAttendanceStatus.Registered;

        [DataType(DataType.Date)]
        public DateTime? AttendedDate { get; set; }

        [MaxLength(500)]
        public string Feedback { get; set; }

        public decimal? Score { get; set; }

        [MaxLength(200)]
        public string CertificatePath { get; set; }

        public DateTime? CertificateIssuedDate { get; set; }

        [MaxLength(500)]
        public string Remarks { get; set; }

        public DateTime RegisteredAt { get; set; } = DateTime.Now;
    }

    public class TrainingEvaluation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TrainingId { get; set; }

        [ForeignKey("TrainingId")]
        public virtual Training Training { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }

        [Range(1, 5)]
        public int OverallRating { get; set; }

        [Range(1, 5)]
        public int ContentRating { get; set; }

        [Range(1, 5)]
        public int TrainerRating { get; set; }

        [Range(1, 5)]
        public int MaterialRating { get; set; }

        [Range(1, 5)]
        public int VenueRating { get; set; }

        [MaxLength(1000)]
        public string WhatLiked { get; set; }

        [MaxLength(1000)]
        public string ImprovementsSuggested { get; set; }

        public bool WouldRecommend { get; set; }

        [MaxLength(500)]
        public string KeyTakeaways { get; set; }

        public DateTime EvaluatedAt { get; set; } = DateTime.Now;
    }

    public enum TrainingType
    {
        Classroom,
        Online,
        Workshop,
        Seminar,
        Conference,
        OnTheJob,
        Mentoring,
        SelfPaced
    }

    public enum TrainingCategory
    {
        Technical,
        SoftSkills,
        Leadership,
        Compliance,
        Safety,
        ProductKnowledge,
        CustomerService,
        Sales,
        Management,
        IT,
        Language,
        Other
    }

    public enum TrainingStatus
    {
        Scheduled,
        Ongoing,
        Completed,
        Cancelled,
        Postponed
    }

    public enum TrainingAttendanceStatus
    {
        Registered,
        Attended,
        Absent,
        Cancelled,
        Transferred
    }
}