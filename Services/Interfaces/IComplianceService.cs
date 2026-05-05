using HR_Management_System.Models;
using HR_Management_System.Models.ViewModels;

namespace HR_Management_System.Services.Interfaces
{
    public interface IComplianceService
    {
        Task<LaborAuditReport> GenerateLaborAuditReportAsync(AuditReportGenerateViewModel model, int generatedBy);
        Task<AccidentLog> LogAccidentAsync(AccidentLogCreateViewModel model);
        Task<AccidentLog> UpdateAccidentLogAsync(AccidentLog log);
        Task<IEnumerable<AccidentLog>> GetAccidentLogsAsync(DateTime? from = null, DateTime? to = null);
        Task<MedicalInsuranceClaim> SubmitInsuranceClaimAsync(MedicalClaimCreateViewModel model);
        Task<bool> ApproveInsuranceClaimAsync(int claimId, int approvedBy, decimal approvedAmount, string remarks);
        Task<bool> RejectInsuranceClaimAsync(int claimId, int rejectedBy, string reason);
        Task<IEnumerable<MedicalInsuranceClaim>> GetPendingClaimsAsync();
        Task<DisciplinaryRecord> CreateDisciplinaryRecordAsync(DisciplinaryCreateViewModel model, int issuedBy);
        Task<bool> ResolveDisciplinaryCaseAsync(int recordId, string resolution);
        Task<IEnumerable<DisciplinaryRecord>> GetUnresolvedDisciplinaryCasesAsync();
        Task<int> GetOpenAccidentCountAsync();
        Task<int> GetPendingInsuranceClaimCountAsync();
        Task<int> GetUnresolvedDisciplinaryCountAsync();
    }
}
