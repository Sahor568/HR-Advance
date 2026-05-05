using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR_Management_System.Models.ViewModels
{
    // ==================== Employee ViewModels ====================

    public class EmployeeCreateViewModel
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? MiddleName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public int Gender { get; set; }

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
        public string Email { get; set; } = string.Empty;

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

        [Required]
        public DateTime Join_Date { get; set; }

        [Required]
        public int EmploymentType { get; set; }

        [Required]
        [StringLength(100)]
        public string Designation { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Grade { get; set; } = string.Empty;

        [Required]
        public decimal BaseSalary { get; set; }

        public decimal GradeAmount { get; set; }

        public bool HasProbation { get; set; } = true;

        public decimal ProbationMonths { get; set; } = 6;

        [StringLength(100)]
        public string? BankName { get; set; }

        [StringLength(50)]
        public string? BankAccountNumber { get; set; }

        [StringLength(100)]
        public string? EmergencyContactName { get; set; }

        [StringLength(20)]
        public string? EmergencyContactPhone { get; set; }

        [StringLength(50)]
        public string? EmergencyContactRelation { get; set; }

        public int MaritalStatus { get; set; }

        [StringLength(100)]
        public string? FatherName { get; set; }

        [StringLength(100)]
        public string? MotherName { get; set; }

        [StringLength(100)]
        public string? SpouseName { get; set; }
    }

    public class EmployeeEditViewModel : EmployeeCreateViewModel
    {
        public int Id { get; set; }
        public string Emp_ID { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int ProbationStatus { get; set; }
        public DateTime? Probation_End { get; set; }
    }

    public class EmployeeListViewModel
    {
        public int Id { get; set; }
        public string Emp_ID { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string EmploymentType { get; set; } = string.Empty;
        public string ProbationStatus { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime Join_Date { get; set; }
    }

    public class EmployeeDetailViewModel
    {
        public Employee Employee { get; set; } = null!;
        public LeaveBalance? CurrentLeaveBalance { get; set; }
        public EmploymentContract? ActiveContract { get; set; }
        public SSFRecord? LatestSSFRecord { get; set; }
        public Payroll? LatestPayroll { get; set; }
        public int TotalAttendanceThisMonth { get; set; }
        public decimal TotalOTHoursThisMonth { get; set; }
    }

    // ==================== Attendance ViewModels ====================

    public class ClockInViewModel
    {
        [Required]
        public int EmployeeId { get; set; }
    }

    public class ClockOutViewModel
    {
        [Required]
        public int EmployeeId { get; set; }
    }

    public class AttendanceFilterViewModel
    {
        public int? EmployeeId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? Status { get; set; }
        public string? Department { get; set; }
    }

    public class AttendanceListViewModel
    {
        public int Id { get; set; }
        public string Emp_ID { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public TimeSpan Clock_In { get; set; }
        public TimeSpan? Clock_Out { get; set; }
        public decimal TotalHours { get; set; }
        public decimal OT_Hours { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class WeeklyWorkHoursViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public decimal TotalHoursWorked { get; set; }
        public decimal OvertimeHours { get; set; }
        public bool IsCompliant => TotalHoursWorked <= 48;
        public string ViolationMessage => TotalHoursWorked > 48 ? $"Exceeds 48-hour weekly limit by {TotalHoursWorked - 48} hours" : "Compliant";
    }

    // ==================== Leave ViewModels ====================

    public class LeaveRequestViewModel
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int LeaveType { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [StringLength(1000)]
        public string? Reason { get; set; }

        public string? DocumentPath { get; set; }
    }

    public class LeaveApprovalViewModel
    {
        [Required]
        public int LeaveRequestId { get; set; }

        [Required]
        public bool IsApproved { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
    }

    public class LeaveBalanceViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal HomeLeaveAccrued { get; set; }
        public decimal HomeLeaveUsed { get; set; }
        public decimal HomeLeaveBalance { get; set; }
        public decimal SickLeaveTotal { get; set; }
        public decimal SickLeaveTaken { get; set; }
        public decimal SickLeaveBalance { get; set; }
        public decimal MaternityLeaveTotal { get; set; }
        public decimal MaternityLeaveUsed { get; set; }
        public decimal PaternityLeaveTotal { get; set; }
        public decimal PaternityLeaveUsed { get; set; }
        public decimal MourningLeaveTotal { get; set; }
        public decimal MourningLeaveUsed { get; set; }
        public decimal PublicHolidaysTotal { get; set; }
        public decimal PublicHolidaysUsed { get; set; }
    }

    public class LeaveEncashmentViewModel
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int LeaveType { get; set; } // Home or Sick

        [Required]
        [Column(TypeName = "decimal(8,2)")]
        public decimal DaysToEncash { get; set; }
    }

    // ==================== Payroll ViewModels ====================

    public class PayrollGenerateViewModel
    {
        [Required]
        public int Month { get; set; }

        [Required]
        public int Year { get; set; }

        public int? EmployeeId { get; set; } // null = generate for all
        public bool IncludeFestivalAllowance { get; set; }
    }

    public class PayrollApprovalViewModel
    {
        [Required]
        public int PayrollId { get; set; }

        [Required]
        public bool IsApproved { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
    }

    public class PayslipViewModel
    {
        public int EmployeeId { get; set; }
        public int PayrollId { get; set; }
        public string Emp_ID { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal GradeAmount { get; set; }
        public decimal OvertimeAmount { get; set; }
        public decimal FestivalAllowance { get; set; }
        public decimal OtherAllowances { get; set; }
        public decimal GrossEarnings { get; set; }
        public decimal SSFEmployeeContribution { get; set; }
        public decimal TaxDeduction { get; set; }
        public decimal OtherDeductions { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetPay { get; set; }
        public decimal WorkingDays { get; set; }
        public decimal DaysWorked { get; set; }
        public decimal OvertimeHours { get; set; }
        public decimal HourlyRate { get; set; }
        public string PayrollStatus { get; set; } = string.Empty;
    }

    // ==================== Contract ViewModels ====================

    public class ContractCreateViewModel
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int EmploymentType { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        [StringLength(100)]
        public string Designation { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required]
        public decimal AgreedSalary { get; set; }

        [StringLength(2000)]
        public string? TermsAndConditions { get; set; }

        [StringLength(2000)]
        public string? JobDescription { get; set; }

        public bool HasProbation { get; set; } = true;

        public decimal ProbationMonths { get; set; } = 6;
    }

    // ==================== Compliance ViewModels ====================

    public class AccidentLogCreateViewModel
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public DateTime AccidentDate { get; set; }

        [Required]
        public TimeSpan AccidentTime { get; set; }

        [Required]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public int Severity { get; set; }

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? ImmediateActionTaken { get; set; }

        [StringLength(1000)]
        public string? RootCause { get; set; }

        [StringLength(2000)]
        public string? PreventiveMeasures { get; set; }

        [StringLength(200)]
        public string? WitnessNames { get; set; }

        [StringLength(1000)]
        public string? InjuryDetails { get; set; }

        public bool RequiredHospitalization { get; set; }
    }

    public class MedicalClaimCreateViewModel
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        [StringLength(200)]
        public string ClaimType { get; set; } = string.Empty;

        [Required]
        public decimal ClaimAmount { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(2000)]
        public string? HospitalDetails { get; set; }

        public DateTime? TreatmentStartDate { get; set; }
        public DateTime? TreatmentEndDate { get; set; }
    }

    public class DisciplinaryCreateViewModel
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int ActionType { get; set; }

        [Required]
        public DateTime IncidentDate { get; set; }

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        public bool IsSection131Compliance { get; set; }

        [StringLength(1000)]
        public string? Section131Details { get; set; }

        [StringLength(2000)]
        public string? WarningDetails { get; set; }

        public DateTime? SuspensionStartDate { get; set; }
        public DateTime? SuspensionEndDate { get; set; }
    }

    // ==================== Audit Report ViewModels ====================

    public class AuditReportGenerateViewModel
    {
        [Required]
        public int ReportType { get; set; }

        [Required]
        public DateTime PeriodStart { get; set; }

        [Required]
        public DateTime PeriodEnd { get; set; }

        [StringLength(2000)]
        public string? Remarks { get; set; }
    }

    // ==================== Leave Encashment Result ====================

    public class LeaveEncashmentResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal EncashmentAmount { get; set; }
        public decimal DaysEncashed { get; set; }
        public int LeaveType { get; set; }
    }

    // ==================== SSF ViewModels ====================

    public class SSFBalanceViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public decimal TotalEmployeeContribution { get; set; }
        public decimal TotalEmployerContribution { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal LastContributionDate { get; set; }
    }

    // ==================== Dashboard ViewModels ====================

    public class DashboardViewModel
    {
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public int EmployeesOnProbation { get; set; }
        public int PendingLeaveRequests { get; set; }
        public int TodayAttendance { get; set; }
        public int PendingPayrolls { get; set; }
        public int OpenAccidentLogs { get; set; }
        public int PendingInsuranceClaims { get; set; }
        public int UnresolvedDisciplinaryCases { get; set; }
        public decimal TotalMonthlyPayroll { get; set; }
        public List<ProbationExpiryViewModel> UpcomingProbationExpiries { get; set; } = new();
    }

    public class ProbationExpiryViewModel
    {
        public int EmployeeId { get; set; }
        public string Emp_ID { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime ProbationEndDate { get; set; }
        public int DaysRemaining { get; set; }
    }

    // ==================== Audit Log ViewModels ====================

    public class AuditLogFilterViewModel
    {
        public string? Level { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class AuditLogListViewModel
    {
        public List<AuditLog> Logs { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    // ==================== Department ViewModels ====================

    public class DepartmentCreateViewModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Code { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? DepartmentHead { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class DepartmentEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Code { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? DepartmentHead { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class DepartmentListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public string? DepartmentHead { get; set; }
        public int EmployeeCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class DepartmentDetailViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public string? DepartmentHead { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public List<DepartmentEmployeeViewModel> Employees { get; set; } = new();
    }

    public class DepartmentEmployeeViewModel
    {
        public int Id { get; set; }
        public string Emp_ID { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}

