using HR_Management_System.Models;
using HR_Management_System.Models.ViewModels;

namespace HR_Management_System.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeListViewModel>> GetAllEmployeesAsync();
        Task<EmployeeDetailViewModel?> GetEmployeeByIdAsync(int id);
        Task<Employee> CreateEmployeeAsync(EmployeeCreateViewModel model);
        Task<Employee> UpdateEmployeeAsync(EmployeeEditViewModel model);
        Task<Employee> UpdateEmployeeAsync(int id, Employee employee);
        Task<bool> DeactivateEmployeeAsync(int id, string reason);
        Task<bool> ActivateEmployeeAsync(int id);
        Task<string> GenerateEmployeeIdAsync();
        Task<IEnumerable<EmployeeListViewModel>> GetEmployeesOnProbationAsync();
        Task<IEnumerable<ProbationExpiryViewModel>> GetUpcomingProbationExpiriesAsync(int daysAhead = 30);
        Task<bool> CompleteProbationAsync(int employeeId);
        Task<bool> ExtendProbationAsync(int employeeId, DateTime newEndDate);
        Task<int> GetTotalActiveEmployeesCountAsync();
        Task<int> GetEmployeesOnProbationCountAsync();
        Task<IEnumerable<EmployeeListViewModel>> SearchEmployeesAsync(string searchTerm);
        Task<Employee?> GetEmployeeByUserIdAsync(string userId);
    }
}
