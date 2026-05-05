namespace HR_Management_System.Models.Enums
{
    public enum EmploymentType
    {
        Regular = 1,
        TimeBound = 2,
        TaskBased = 3,
        Casual = 4,
        PartTime = 5
    }

    public enum Gender
    {
        Male = 1,
        Female = 2,
        Other = 3
    }

    public enum ProbationStatus
    {
        Active = 1,
        Completed = 2,
        Extended = 3,
        Terminated = 4
    }

    public enum AttendanceStatus
    {
        Present = 1,
        Absent = 2,
        HalfDay = 3,
        OnLeave = 4,
        Holiday = 5,
        Weekend = 6
    }

    public enum LeaveStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        Cancelled = 4
    }

    public enum LeaveTypeEnum
    {
        Home = 1,
        Sick = 2,
        Maternity = 3,
        Paternity = 4,
        Mourning = 5,
        PublicHoliday = 6
    }

    public enum DisciplinaryActionType
    {
        VerbalWarning = 1,
        WrittenWarning = 2,
        Suspension = 3,
        Termination = 4,
        MisconductSection131 = 5
    }

    public enum ClaimStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        Processing = 4
    }

    public enum AccidentSeverity
    {
        Minor = 1,
        Moderate = 2,
        Major = 3,
        Fatal = 4
    }

    public enum ContractStatus
    {
        Active = 1,
        Expired = 2,
        Terminated = 3,
        Renewed = 4
    }

    public enum PayrollStatus
    {
        Draft = 1,
        Generated = 2,
        Approved = 3,
        Paid = 4
    }

    public enum MaritalStatus
    {
        Single = 1,
        Married = 2,
        Divorced = 3,
        Widowed = 4
    }

    public enum ReportType
    {
        LaborAudit = 1,
        AttendanceSummary = 2,
        PayrollSummary = 3,
        LeaveReport = 4,
        ComplianceReport = 5,
        OHSReport = 6
    }
}
