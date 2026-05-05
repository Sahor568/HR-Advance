using HR_Management_System.Models;
using HR_Management_System.Models.ViewModels;

namespace HR_Management_System.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<IEnumerable<DepartmentListViewModel>> GetAllDepartmentsAsync();
        Task<DepartmentDetailViewModel?> GetDepartmentByIdAsync(int id);
        Task<Department> CreateDepartmentAsync(DepartmentCreateViewModel model);
        Task<Department> UpdateDepartmentAsync(DepartmentEditViewModel model);
        Task<bool> DeleteDepartmentAsync(int id);
        Task<bool> ToggleDepartmentStatusAsync(int id, bool isActive);
        Task<int> GetDepartmentEmployeeCountAsync(int departmentId);
        Task<IEnumerable<DepartmentListViewModel>> SearchDepartmentsAsync(string searchTerm);
        Task<IEnumerable<DepartmentListViewModel>> GetActiveDepartmentsAsync();
    }
}