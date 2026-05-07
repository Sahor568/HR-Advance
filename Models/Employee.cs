using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HR_Management_System.Models.Enums;

namespace HR_Management_System.Models
{
    [Table("Employees")]
    public class Employee
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Emp_ID { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? MiddleName { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {MiddleName} {LastName}".Replace("  ", " ").Trim();

        [Required]
        public Gender Gender { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(200)]
        public string PermanentAddress { get; set; } = string.Empty;

        [StringLength(200)]
        public string? TemporaryAddress { get; set; }

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(20)]
        public string? AlternatePhone { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        // Citizenship / Identity Details for Compliance
        [Required]
        [StringLength(50)]
        public string CitizenshipNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CitizenshipIssuedDistrict { get; set; } = string.Empty;

        [Required]
        public DateTime CitizenshipIssuedDate { get; set; }

        [StringLength(50)]
        public string? PAN_No { get; set; }

        [StringLength(50)]
        public string? SocialSecurityNumber { get; set; }

        // Employment Details
        [Required]
        public DateTime Join_Date { get; set; }

        [Required]
        public EmploymentType EmploymentType { get; set; }

        [Required]
        [StringLength(100)]
        public string Designation { get; set; } = string.Empty;

        // Foreign key to Department
        public int? DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department? DepartmentEntity { get; set; }

        // Keep string Department for backward compatibility (can be computed from DepartmentEntity)
        [Required]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Grade { get; set; } = string.Empty;

        // Probation Tracking
        public DateTime? Probation_End { get; set; }

        public ProbationStatus ProbationStatus { get; set; } = ProbationStatus.Active;

        public DateTime? ProbationReminderSentDate { get; set; }

        // Salary Info
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GradeAmount { get; set; }

        // Bank Details
        [StringLength(100)]
        public string? BankName { get; set; }

        [StringLength(50)]
        public string? BankAccountNumber { get; set; }

        // Emergency Contact
        [StringLength(100)]
        public string? EmergencyContactName { get; set; }

        [StringLength(20)]
        public string? EmergencyContactPhone { get; set; }

        [StringLength(50)]
        public string? EmergencyContactRelation { get; set; }

        [StringLength(50)]
        public string? EmergencyContactRelationship { get; set; }

        // Marital Status
        public MaritalStatus MaritalStatus { get; set; }

        // Father's / Mother's Name (Nepal compliance)
        [StringLength(100)]
        public string? FatherName { get; set; }

        [StringLength(100)]
        public string? MotherName { get; set; }

        // Spouse Name
        [StringLength(100)]
        public string? SpouseName { get; set; }

        // Status
        public bool IsActive { get; set; } = true;

        public DateTime? TerminationDate { get; set; }

        [StringLength(500)]
        public string? TerminationReason { get; set; }

        // Photo
        [StringLength(500)]
        public string? PhotoPath { get; set; }

        // KYC Documents
        [StringLength(500)]
        public string? CVPath { get; set; }

        [StringLength(500)]
        public string? ExperienceCertificatePath { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Supervisor relationship (optional)
        public int? SupervisorId { get; set; }

        [ForeignKey("SupervisorId")]
        public virtual Employee? Supervisor { get; set; }

        // Navigation Properties
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
        public virtual ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
        public virtual ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
        public virtual ICollection<EmploymentContract> Contracts { get; set; } = new List<EmploymentContract>();
        public virtual ICollection<DisciplinaryRecord> DisciplinaryRecords { get; set; } = new List<DisciplinaryRecord>();
        public virtual ICollection<AccidentLog> AccidentLogs { get; set; } = new List<AccidentLog>();
        public virtual ICollection<MedicalInsuranceClaim> MedicalInsuranceClaims { get; set; } = new List<MedicalInsuranceClaim>();
        public virtual ICollection<SSFRecord> SSFRecords { get; set; } = new List<SSFRecord>();
        public virtual ICollection<Employee> Subordinates { get; set; } = new List<Employee>();
    }
}
