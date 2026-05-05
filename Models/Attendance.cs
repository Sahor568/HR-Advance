using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HR_Management_System.Models.Enums;

namespace HR_Management_System.Models
{
    [Table("Attendances")]
    public class Attendance
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public TimeSpan Clock_In { get; set; }

        public TimeSpan? Clock_Out { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal OT_Hours { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal TotalHours { get; set; }

        [Required]
        public AttendanceStatus Status { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        // Tracking
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }
    }
}
