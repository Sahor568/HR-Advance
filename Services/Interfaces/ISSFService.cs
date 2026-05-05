using HR_Management_System.Models;
using HR_Management_System.Models.ViewModels;

namespace HR_Management_System.Services.Interfaces
{
    public interface ISSFService
    {
        Task<SSFBalanceViewModel> GetSSFBalanceAsync(int employeeId);
        Task<IEnumerable<SSFRecord>> GetSSFHistoryAsync(int employeeId);
        Task<decimal> GetTotalEmployeeContributionAsync(int employeeId);
        Task<decimal> GetTotalEmployerContributionAsync(int employeeId);
    }
}
