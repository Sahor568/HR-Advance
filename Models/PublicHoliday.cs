using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR_Management_System.Models
{
    [Table("PublicHolidays")]
    public class PublicHoliday
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string HolidayName { get; set; } = string.Empty;

        [Required]
        public DateTime HolidayDate { get; set; }

        [StringLength(50)]
        public string? NepaliDate { get; set; }

        public bool IsForWomenOnly { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
