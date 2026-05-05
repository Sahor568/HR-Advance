using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HR_Management_System.Models.Identity;

namespace HR_Management_System.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; }

        [Required]
        public NotificationType Type { get; set; }

        [Required]
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

        [MaxLength(100)]
        public string Module { get; set; }

        [MaxLength(100)]
        public string Action { get; set; }

        [MaxLength(500)]
        public string ReferenceId { get; set; }

        [MaxLength(500)]
        public string LinkUrl { get; set; }

        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }

        public bool IsActionRequired { get; set; } = false;

        [MaxLength(100)]
        public string ActionButtonText { get; set; }

        [MaxLength(500)]
        public string ActionUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ExpiresAt { get; set; }

        [MaxLength(450)]
        public string CreatedBy { get; set; }

        public bool IsArchived { get; set; } = false;
        public DateTime? ArchivedAt { get; set; }
    }

    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error,
        Approval,
        Reminder,
        Alert,
        System
    }

    public enum NotificationPriority
    {
        Low,
        Normal,
        High,
        Critical
    }
}