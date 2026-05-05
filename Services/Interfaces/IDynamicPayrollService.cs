using HR_Management_System.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HR_Management_System.Services.Interfaces
{
    public interface IDynamicPayrollService
    {
        // Payroll Configuration Management
        Task<PayrollConfiguration> CreatePayrollConfigurationAsync(PayrollConfiguration configuration);
        Task<PayrollConfiguration> GetPayrollConfigurationByIdAsync(int id);
        Task<IEnumerable<PayrollConfiguration>> GetAllPayrollConfigurationsAsync(bool activeOnly = true);
        Task<PayrollConfiguration> UpdatePayrollConfigurationAsync(int id, PayrollConfiguration configuration);
        Task<bool> DeletePayrollConfigurationAsync(int id);
        Task<bool> ActivatePayrollConfigurationAsync(int id);
        Task<bool> DeactivatePayrollConfigurationAsync(int id);
        
        // Payroll Component Management
        Task<PayrollComponent> CreatePayrollComponentAsync(PayrollComponent component);
        Task<PayrollComponent> GetPayrollComponentByIdAsync(int id);
        Task<IEnumerable<PayrollComponent>> GetAllPayrollComponentsAsync(int configurationId);
        Task<PayrollComponent> UpdatePayrollComponentAsync(int id, PayrollComponent component);
        Task<bool> DeletePayrollComponentAsync(int id);
        Task<bool> ReorderPayrollComponentsAsync(int configurationId, Dictionary<int, int> componentOrder);
        
        // Tax Configuration
        Task<TaxSlab> CreateTaxSlabAsync(TaxSlab taxSlab);
        Task<TaxSlab> GetTaxSlabByIdAsync(int id);
        Task<IEnumerable<TaxSlab>> GetAllTaxSlabsAsync(bool activeOnly = true);
        Task<TaxSlab> UpdateTaxSlabAsync(int id, TaxSlab taxSlab);
        Task<bool> DeleteTaxSlabAsync(int id);
        Task<bool> ActivateTaxSlabAsync(int id);
        Task<bool> DeactivateTaxSlabAsync(int id);
        Task<decimal> CalculateTaxAsync(decimal taxableIncome, DateTime effectiveDate);
        
        // SSF Configuration
        Task<SSFConfiguration> CreateSSFConfigurationAsync(SSFConfiguration configuration);
        Task<SSFConfiguration> GetSSFConfigurationByIdAsync(int id);
        Task<IEnumerable<SSFConfiguration>> GetAllSSFConfigurationsAsync(bool activeOnly = true);
        Task<SSFConfiguration> UpdateSSFConfigurationAsync(int id, SSFConfiguration configuration);
        Task<bool> DeleteSSFConfigurationAsync(int id);
        Task<SSFConfiguration> GetActiveSSFConfigurationAsync();
        Task<object> CalculateSSFContributionAsync(decimal basicSalary, DateTime effectiveDate);
        
        // Dynamic Payroll Calculation
        Task<object> CalculateEmployeePayrollAsync(int employeeId, int month, int year, int configurationId);
        Task<object> CalculatePayrollComponentAsync(int employeeId, string componentType, decimal baseAmount, Dictionary<string, object> parameters);
        Task<object> ValidatePayrollCalculationAsync(int employeeId, int month, int year);
        Task<object> SimulatePayrollChangeAsync(int employeeId, Dictionary<string, object> changes);
        
        // Payroll Processing
        Task<object> ProcessPayrollBatchAsync(List<int> employeeIds, int month, int year, int configurationId);
        Task<object> GeneratePayslipsAsync(List<int> employeeIds, int month, int year);
        Task<bool> ApprovePayrollBatchAsync(int batchId, string approvedBy);
        Task<bool> RejectPayrollBatchAsync(int batchId, string rejectedBy, string reason);
        Task<bool> ProcessPayrollPaymentAsync(int batchId, string paymentMethod);
        
        // Loan & Advance Management
        Task<object> CreateLoanAsync(int employeeId, decimal amount, int installments, decimal interestRate, string purpose);
        Task<object> CreateAdvanceAsync(int employeeId, decimal amount, string purpose, DateTime recoveryStartDate);
        Task<IEnumerable<object>> GetEmployeeLoansAsync(int employeeId, bool activeOnly = true);
        Task<IEnumerable<object>> GetEmployeeAdvancesAsync(int employeeId, bool activeOnly = true);
        Task<bool> ProcessLoanDeductionAsync(int employeeId, int month, int year);
        Task<bool> ProcessAdvanceDeductionAsync(int employeeId, int month, int year);
        
        // Arrears & Bonus Management
        Task<object> CreateArrearsAsync(int employeeId, decimal amount, string reason, DateTime effectiveFrom);
        Task<object> CreateBonusAsync(int employeeId, decimal amount, string bonusType, string reason);
        Task<IEnumerable<object>> GetEmployeeArrearsAsync(int employeeId, bool pendingOnly = true);
        Task<IEnumerable<object>> GetEmployeeBonusesAsync(int employeeId, int? year = null);
        
        // Payroll Compliance
        Task<object> GeneratePFReportAsync(int month, int year);
        Task<object> GenerateTDSReportAsync(int month, int year);
        Task<object> GenerateSSFReportAsync(int month, int year);
        Task<object> GeneratePayrollRegisterAsync(int month, int year);
        Task<object> GenerateBankPaymentFileAsync(int month, int year, string bankFormat);
        
        // Payroll Analytics
        Task<object> GetPayrollAnalyticsAsync(int? year = null, int? month = null);
        Task<object> GetPayrollCostAnalysisAsync(int? departmentId = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<object> GetPayrollTrendAnalysisAsync(int employeeId, int years = 3);
        Task<object> GetComponentWiseAnalysisAsync(string componentType, DateTime startDate, DateTime endDate);
        
        // Custom Formula Engine
        Task<object> EvaluatePayrollFormulaAsync(string formula, Dictionary<string, object> variables);
        Task<object> CreateCustomFormulaAsync(string formulaName, string formulaExpression, Dictionary<string, string> parameters);
        Task<object> ValidateFormulaSyntaxAsync(string formula);
        
        // Integration & Export
        Task<bool> ExportPayrollDataAsync(int month, int year, string format, Dictionary<string, object> options);
        Task<bool> IntegrateWithAccountingSoftwareAsync(int month, int year, string softwareName);
        Task<bool> SyncWithBankAPIAsync(int month, int year, string bankName);
    }
}