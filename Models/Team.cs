using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR_Management_System.Models
{
    [Table("Teams")]
    public class Team
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }

        [Required]
        public int CreatedBy { get; set; } // Employee ID who created the team

        [ForeignKey("CreatedBy")]
        public virtual Employee? Creator { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
        public virtual ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
    }

    [Table("TeamMembers")]
    public class TeamMember
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TeamId { get; set; }

        [ForeignKey("TeamId")]
        public virtual Team? Team { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }

        [Required]
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LeftAt { get; set; }

        [StringLength(50)]
        public string Role { get; set; } = "Member"; // Team Lead, Member, etc.

        public bool IsActive { get; set; } = true;
    }

    [Table("ProjectTasks")]
    public class ProjectTask
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public int TeamId { get; set; }

        [ForeignKey("TeamId")]
        public virtual Team? Team { get; set; }

        [Required]
        public int AssignedBy { get; set; } // HR/Admin user ID

        public int? AssignedTo { get; set; } // Specific team member (optional)

        [ForeignKey("AssignedTo")]
        public virtual Employee? AssignedEmployee { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime Deadline { get; set; }

        public DateTime? CompletedAt { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, In Progress, Completed, Cancelled

        [Required]
        public int Priority { get; set; } = 1; // 1=Low, 2=Medium, 3=High, 4=Critical

        [StringLength(500)]
        public string? Notes { get; set; }

        // Extension related fields
        public DateTime? ExtendedDeadline { get; set; }
        public int ExtensionRequestCount { get; set; } = 0;
        public bool PerformancePenaltyApplied { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation property for extension requests
        public virtual ICollection<TaskExtensionRequest>? ExtensionRequests { get; set; }
    }
}