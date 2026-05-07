using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR_Management_System.Models
{
    [Table("AuditLogs")]
    public class AuditLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [StringLength(100)]
        public string? Level { get; set; }

        public DateTime TimeStamp { get; set; }

        [StringLength(500)]
        public string? Message { get; set; }

        [StringLength(500)]
        public string? MessageTemplate { get; set; }

        [StringLength(200)]
        public string? SourceContext { get; set; }

        [StringLength(100)]
        public string? RequestId { get; set; }

        [StringLength(100)]
        public string? TraceId { get; set; }

        [StringLength(50)]
        public string? SpanId { get; set; }

        [StringLength(200)]
        public string? UserName { get; set; }

        [StringLength(200)]
        public string? Action { get; set; }

        [StringLength(200)]
        public string? Controller { get; set; }

        [StringLength(50)]
        public string? HttpMethod { get; set; }

        [StringLength(500)]
        public string? RequestPath { get; set; }

        [StringLength(int.MaxValue)]
        public string? Exception { get; set; }

        [StringLength(int.MaxValue)]
        public string? Properties { get; set; }
    }
}
