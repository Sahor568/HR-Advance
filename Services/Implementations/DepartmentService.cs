using Microsoft.EntityFrameworkCore;
using HR_Management_System.Data;
using HR_Management_System.Models;
using HR_Management_System.Models.ViewModels;
using HR_Management_System.Services.Interfaces;

namespace HR_Management_System.Services.Implementations
{
    public class DepartmentService : IDepartmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DepartmentService> _logger;

        public DepartmentService(ApplicationDbContext context, ILogger<DepartmentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<DepartmentListViewModel>> GetAllDepartmentsAsync()
        {
            _logger.LogInformation("Fetching all departments");
            return await _context.Departments
                .Select(d => new DepartmentListViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    Code = d.Code,
                    Description = d.Description,
                    DepartmentHead = d.DepartmentHead,
                    EmployeeCount = d.Employees.Count(e => e.IsActive),
                    IsActive = d.IsActive,
                    CreatedDate = d.CreatedDate
                })
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<DepartmentDetailViewModel?> GetDepartmentByIdAsync(int id)
        {
            _logger.LogInformation("Fetching department details for ID: {DepartmentId}", id);
            var department = await _context.Departments
                .Include(d => d.Employees.Where(e => e.IsActive))
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
            {
                _logger.LogWarning("Department not found with ID: {DepartmentId}", id);
                return null;
            }

            return new DepartmentDetailViewModel
            {
                Id = department.Id,
                Name = department.Name,
                Code = department.Code,
                Description = department.Description,
                DepartmentHead = department.DepartmentHead,
                IsActive = department.IsActive,
                CreatedDate = department.CreatedDate,
                ModifiedDate = department.ModifiedDate,
                Employees = department.Employees.Select(e => new DepartmentEmployeeViewModel
                {
                    Id = e.Id,
                    Emp_ID = e.Emp_ID,
                    FullName = e.FullName,
                    Designation = e.Designation,
                    Email = e.Email,
                    PhoneNumber = e.PhoneNumber,
                    IsActive = e.IsActive
                }).ToList()
            };
        }

        public async Task<Department> CreateDepartmentAsync(DepartmentCreateViewModel model)
        {
            _logger.LogInformation("Creating new department: {DepartmentName}", model.Name);

            // Check if department with same name or code already exists
            if (await _context.Departments.AnyAsync(d => d.Name == model.Name))
            {
                throw new InvalidOperationException($"Department with name '{model.Name}' already exists.");
            }

            if (!string.IsNullOrEmpty(model.Code) && await _context.Departments.AnyAsync(d => d.Code == model.Code))
            {
                throw new InvalidOperationException($"Department with code '{model.Code}' already exists.");
            }

            var department = new Department
            {
                Name = model.Name,
                Code = model.Code,
                Description = model.Description,
                DepartmentHead = model.DepartmentHead,
                IsActive = model.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Department created successfully with ID: {DepartmentId}", department.Id);
            return department;
        }

        public async Task<Department> UpdateDepartmentAsync(DepartmentEditViewModel model)
        {
            _logger.LogInformation("Updating department with ID: {DepartmentId}", model.Id);
            var department = await _context.Departments.FindAsync(model.Id);

            if (department == null)
            {
                _logger.LogWarning("Department not found with ID: {DepartmentId}", model.Id);
                throw new KeyNotFoundException($"Department with ID {model.Id} not found.");
            }

            // Check for duplicate name (excluding current department)
            if (await _context.Departments.AnyAsync(d => d.Name == model.Name && d.Id != model.Id))
            {
                throw new InvalidOperationException($"Department with name '{model.Name}' already exists.");
            }

            // Check for duplicate code (excluding current department)
            if (!string.IsNullOrEmpty(model.Code) && 
                await _context.Departments.AnyAsync(d => d.Code == model.Code && d.Id != model.Id))
            {
                throw new InvalidOperationException($"Department with code '{model.Code}' already exists.");
            }

            department.Name = model.Name;
            department.Code = model.Code;
            department.Description = model.Description;
            department.DepartmentHead = model.DepartmentHead;
            department.IsActive = model.IsActive;
            department.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Department updated successfully with ID: {DepartmentId}", department.Id);
            return department;
        }

        public async Task<bool> DeleteDepartmentAsync(int id)
        {
            _logger.LogInformation("Deleting department with ID: {DepartmentId}", id);
            var department = await _context.Departments
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
            {
                _logger.LogWarning("Department not found with ID: {DepartmentId}", id);
                return false;
            }

            // Check if department has employees
            if (department.Employees.Any(e => e.IsActive))
            {
                _logger.LogWarning("Cannot delete department with active employees. Department ID: {DepartmentId}", id);
                throw new InvalidOperationException("Cannot delete department that has active employees. Deactivate the department instead.");
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Department deleted successfully with ID: {DepartmentId}", id);
            return true;
        }

        public async Task<bool> ToggleDepartmentStatusAsync(int id, bool isActive)
        {
            _logger.LogInformation("Toggling department status to {Status} for ID: {DepartmentId}", isActive ? "Active" : "Inactive", id);
            var department = await _context.Departments.FindAsync(id);

            if (department == null)
            {
                _logger.LogWarning("Department not found with ID: {DepartmentId}", id);
                return false;
            }

            department.IsActive = isActive;
            department.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Department status updated successfully for ID: {DepartmentId}", id);
            return true;
        }

        public async Task<int> GetDepartmentEmployeeCountAsync(int departmentId)
        {
            return await _context.Employees
                .CountAsync(e => e.DepartmentId == departmentId && e.IsActive);
        }

        public async Task<IEnumerable<DepartmentListViewModel>> SearchDepartmentsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllDepartmentsAsync();

            var lowerTerm = searchTerm.ToLower();
            return await _context.Departments
                .Where(d => d.Name.ToLower().Contains(lowerTerm) ||
                           (d.Code != null && d.Code.ToLower().Contains(lowerTerm)) ||
                           (d.Description != null && d.Description.ToLower().Contains(lowerTerm)) ||
                           (d.DepartmentHead != null && d.DepartmentHead.ToLower().Contains(lowerTerm)))
                .Select(d => new DepartmentListViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    Code = d.Code,
                    Description = d.Description,
                    DepartmentHead = d.DepartmentHead,
                    EmployeeCount = d.Employees.Count(e => e.IsActive),
                    IsActive = d.IsActive,
                    CreatedDate = d.CreatedDate
                })
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<DepartmentListViewModel>> GetActiveDepartmentsAsync()
        {
            return await _context.Departments
                .Where(d => d.IsActive)
                .Select(d => new DepartmentListViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    Code = d.Code,
                    Description = d.Description,
                    DepartmentHead = d.DepartmentHead,
                    EmployeeCount = d.Employees.Count(e => e.IsActive),
                    IsActive = d.IsActive,
                    CreatedDate = d.CreatedDate
                })
                .OrderBy(d => d.Name)
                .ToListAsync();
        }
    }
}