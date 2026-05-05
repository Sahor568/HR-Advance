using HR_Management_System.Models;
using HR_Management_System.Models.ViewModels;

namespace HR_Management_System.Services.Interfaces
{
    public interface IPayrollService
    {
        Task<Payroll> GeneratePayrollAsync(int employeeId, int month, int year, bool includeFestivalAllowance = false);
        Task<IEnumerable<Payroll>> GenerateBulkPayrollAsync(int month, int year, bool includeFestivalAllowance = false);
        Task<bool> ApprovePayrollAsync(int payrollId, int approvedBy);
        Task<bool> MarkAsPaidAsync(int payrollId);
        Task<PayslipViewModel> GetPayslipAsync(int payrollId);
        Task<IEnumerable<PayslipViewModel>> GetEmployeePayslipsAsync(int employeeId);
        Task<IEnumerable<PayslipViewModel>> GetEmployeePayslipsAsync(int employeeId, int? year, int? month);
        Task<PayslipViewModel> GetPayslipByIdAsync(int payrollId);
        Task<byte[]> GeneratePayslipPdfAsync(int payrollId);
        Task<decimal> CalculateTDSCalculationAsync(decimal annualTaxableIncome);
        Task<decimal> CalculateOvertimePayAsync(decimal baseSalary, decimal overtimeHours);
        Task<SSFRecord> GenerateSSFRecordAsync(int employeeId, int month, int year);
        Task<IEnumerable<Payroll>> GetPayrollsByMonthYearAsync(int month, int year);
        Task<int> GetPendingPayrollCountAsync();
        Task<decimal> GetTotalMonthlyPayrollAsync(int month, int year);
    }
}
