using HR_Management_System.Models;
using HR_Management_System.Models.ViewModels;

namespace HR_Management_System.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task<AuditLogListViewModel> GetAuditLogsAsync(AuditLogFilterViewModel filter);
        Task<AuditLog?> GetAuditLogByIdAsync(int id);
        Task<IEnumerable<AuditLog>> GetRecentLogsAsync(int count = 100);
    }
}
