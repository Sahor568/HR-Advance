using Microsoft.EntityFrameworkCore;
using HR_Management_System.Data;
using HR_Management_System.Models;
using HR_Management_System.Models.Enums;
using HR_Management_System.Models.ViewModels;
using HR_Management_System.Services.Interfaces;

namespace HR_Management_System.Services.Implementations
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(ApplicationDbContext context, ILogger<EmployeeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<EmployeeListViewModel>> GetAllEmployeesAsync()
        {
            _logger.LogInformation("Fetching all employees");
            return await _context.Employees
                .Where(e => e.IsActive)
                .Select(e => new EmployeeListViewModel
                {
                    Id = e.Id,
                    Emp_ID = e.Emp_ID,
                    FullName = e.FullName,
                    Designation = e.Designation,
                    Department = e.Department,
                    EmploymentType = e.EmploymentType.ToString(),
                    ProbationStatus = e.ProbationStatus.ToString(),
                    IsActive = e.IsActive,
                    Join_Date = e.Join_Date
                })
                .OrderByDescending(e => e.Join_Date)
                .ToListAsync();
        }

        public async Task<EmployeeDetailViewModel?> GetEmployeeByIdAsync(int id)
        {
            _logger.LogInformation("Fetching employee details for ID: {EmployeeId}", id);
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                _logger.LogWarning("Employee not found with ID: {EmployeeId}", id);
                return null;
            }

            var currentYear = DateTime.UtcNow.Year;
            var currentMonth = DateTime.UtcNow.Month;

            return new EmployeeDetailViewModel
            {
                Employee = employee,
                CurrentLeaveBalance = await _context.LeaveBalances
                    .FirstOrDefaultAsync(lb => lb.EmployeeId == id && lb.Year == currentYear),
                ActiveContract = await _context.EmploymentContracts
                    .FirstOrDefaultAsync(c => c.EmployeeId == id && c.Status == ContractStatus.Active),
                LatestSSFRecord = await _context.SSFRecords
                    .Where(s => s.EmployeeId == id)
                    .OrderByDescending(s => s.Year).ThenByDescending(s => s.Month)
                    .FirstOrDefaultAsync(),
                LatestPayroll = await _context.Payrolls
                    .Where(p => p.EmployeeId == id)
                    .OrderByDescending(p => p.Year).ThenByDescending(p => p.Month)
                    .FirstOrDefaultAsync(),
                TotalAttendanceThisMonth = await _context.Attendances
                    .CountAsync(a => a.EmployeeId == id && a.Date.Month == currentMonth && a.Date.Year == currentYear && a.Status == AttendanceStatus.Present),
                TotalOTHoursThisMonth = await _context.Attendances
                    .Where(a => a.EmployeeId == id && a.Date.Month == currentMonth && a.Date.Year == currentYear)
                    .SumAsync(a => a.OT_Hours)
            };
        }

        public async Task<Employee> CreateEmployeeAsync(EmployeeCreateViewModel model)
        {
            _logger.LogInformation("Creating new employee with email: {Email}", model.Email);

            var empId = await GenerateEmployeeIdAsync();

            var employee = new Employee
            {
                Emp_ID = empId,
                FirstName = model.FirstName,
                MiddleName = model.MiddleName,
                LastName = model.LastName,
                Gender = (Gender)model.Gender,
                DateOfBirth = model.DateOfBirth,
                PermanentAddress = model.PermanentAddress,
                TemporaryAddress = model.TemporaryAddress,
                PhoneNumber = model.PhoneNumber,
                AlternatePhone = model.AlternatePhone,
                Email = model.Email,
                CitizenshipNumber = model.CitizenshipNumber,
                CitizenshipIssuedDistrict = model.CitizenshipIssuedDistrict,
                CitizenshipIssuedDate = model.CitizenshipIssuedDate,
                PAN_No = model.PAN_No,
                Join_Date = model.Join_Date,
                EmploymentType = (EmploymentType)model.EmploymentType,
                Designation = model.Designation,
                Department = model.Department,
                Grade = model.Grade,
                BaseSalary = model.BaseSalary,
                GradeAmount = model.GradeAmount,
                BankName = model.BankName,
                BankAccountNumber = model.BankAccountNumber,
                EmergencyContactName = model.EmergencyContactName,
                EmergencyContactPhone = model.EmergencyContactPhone,
                EmergencyContactRelation = model.EmergencyContactRelation,
                MaritalStatus = (MaritalStatus)model.MaritalStatus,
                FatherName = model.FatherName,
                MotherName = model.MotherName,
                SpouseName = model.SpouseName,
                PhotoPath = model.PhotoPath,
                CVPath = model.CVPath,
                ExperienceCertificatePath = model.ExperienceCertificatePath,
                SupervisorId = model.SupervisorId,
                IsActive = true,
                ProbationStatus = model.HasProbation ? ProbationStatus.Active : ProbationStatus.Completed,
                Probation_End = model.HasProbation ? model.Join_Date.AddMonths((int)model.ProbationMonths) : null,
                CreatedAt = DateTime.UtcNow
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            // Initialize leave balance for current year
            await InitializeLeaveBalanceForEmployee(employee.Id, model.Join_Date.Year);

            // Create employment contract
            var contract = new EmploymentContract
            {
                EmployeeId = employee.Id,
                ContractNumber = $"CON-{empId}-{DateTime.UtcNow:yyyyMMdd}",
                EmploymentType = (EmploymentType)model.EmploymentType,
                StartDate = model.Join_Date,
                Designation = model.Designation,
                Department = model.Department,
                AgreedSalary = model.BaseSalary + model.GradeAmount,
                HasProbation = model.HasProbation,
                ProbationMonths = model.ProbationMonths,
                Status = ContractStatus.Active,
                CreatedAt = DateTime.UtcNow
            };
            _context.EmploymentContracts.Add(contract);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Employee created successfully with Emp_ID: {EmpId}", empId);
            return employee;
        }

        public async Task<Employee> UpdateEmployeeAsync(EmployeeEditViewModel model)
        {
            _logger.LogInformation("Updating employee ID: {EmployeeId}", model.Id);

            var employee = await _context.Employees.FindAsync(model.Id);
            if (employee == null)
            {
                throw new KeyNotFoundException($"Employee with ID {model.Id} not found");
            }

            employee.FirstName = model.FirstName;
            employee.MiddleName = model.MiddleName;
            employee.LastName = model.LastName;
            employee.Gender = (Gender)model.Gender;
            employee.DateOfBirth = model.DateOfBirth;
            employee.PermanentAddress = model.PermanentAddress;
            employee.TemporaryAddress = model.TemporaryAddress;
            employee.PhoneNumber = model.PhoneNumber;
            employee.AlternatePhone = model.AlternatePhone;
            employee.Email = model.Email;
            employee.CitizenshipNumber = model.CitizenshipNumber;
            employee.CitizenshipIssuedDistrict = model.CitizenshipIssuedDistrict;
            employee.CitizenshipIssuedDate = model.CitizenshipIssuedDate;
            employee.PAN_No = model.PAN_No;
            employee.EmploymentType = (EmploymentType)model.EmploymentType;
            employee.Designation = model.Designation;
            employee.Department = model.Department;
            employee.Grade = model.Grade;
            employee.BaseSalary = model.BaseSalary;
            employee.GradeAmount = model.GradeAmount;
            employee.BankName = model.BankName;
            employee.BankAccountNumber = model.BankAccountNumber;
            employee.EmergencyContactName = model.EmergencyContactName;
            employee.EmergencyContactPhone = model.EmergencyContactPhone;
            employee.EmergencyContactRelation = model.EmergencyContactRelation;
            employee.MaritalStatus = (MaritalStatus)model.MaritalStatus;
            employee.FatherName = model.FatherName;
            employee.MotherName = model.MotherName;
            employee.SpouseName = model.SpouseName;
            employee.PhotoPath = model.PhotoPath;
            employee.CVPath = model.CVPath;
            employee.ExperienceCertificatePath = model.ExperienceCertificatePath;
            employee.SupervisorId = model.SupervisorId;
            employee.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Employee updated successfully: {EmpId}", employee.Emp_ID);
            return employee;
        }

        public async Task<Employee> UpdateEmployeeAsync(int id, Employee employee)
        {
            _logger.LogInformation("Updating employee ID: {EmployeeId}", id);

            var existingEmployee = await _context.Employees.FindAsync(id);
            if (existingEmployee == null)
            {
                throw new KeyNotFoundException($"Employee with ID {id} not found");
            }

            existingEmployee.FirstName = employee.FirstName;
            existingEmployee.MiddleName = employee.MiddleName;
            existingEmployee.LastName = employee.LastName;
            existingEmployee.Gender = employee.Gender;
            existingEmployee.DateOfBirth = employee.DateOfBirth;
            existingEmployee.PermanentAddress = employee.PermanentAddress;
            existingEmployee.TemporaryAddress = employee.TemporaryAddress;
            existingEmployee.PhoneNumber = employee.PhoneNumber;
            existingEmployee.AlternatePhone = employee.AlternatePhone;
            existingEmployee.Email = employee.Email;
            existingEmployee.CitizenshipNumber = employee.CitizenshipNumber;
            existingEmployee.CitizenshipIssuedDistrict = employee.CitizenshipIssuedDistrict;
            existingEmployee.CitizenshipIssuedDate = employee.CitizenshipIssuedDate;
            existingEmployee.PAN_No = employee.PAN_No;
            existingEmployee.EmploymentType = employee.EmploymentType;
            existingEmployee.Designation = employee.Designation;
            existingEmployee.Department = employee.Department;
            existingEmployee.Grade = employee.Grade;
            existingEmployee.BaseSalary = employee.BaseSalary;
            existingEmployee.GradeAmount = employee.GradeAmount;
            existingEmployee.BankName = employee.BankName;
            existingEmployee.BankAccountNumber = employee.BankAccountNumber;
            existingEmployee.EmergencyContactName = employee.EmergencyContactName;
            existingEmployee.EmergencyContactPhone = employee.EmergencyContactPhone;
            existingEmployee.EmergencyContactRelation = employee.EmergencyContactRelation;
            existingEmployee.MaritalStatus = employee.MaritalStatus;
            existingEmployee.FatherName = employee.FatherName;
            existingEmployee.MotherName = employee.MotherName;
            existingEmployee.SpouseName = employee.SpouseName;
            existingEmployee.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Employee updated successfully: {EmpId}", existingEmployee.Emp_ID);
            return existingEmployee;
        }

        public async Task<bool> DeactivateEmployeeAsync(int id, string reason)
        {
            _logger.LogInformation("Deactivating employee ID: {EmployeeId}, Reason: {Reason}", id, reason);

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return false;

            employee.IsActive = false;
            employee.TerminationDate = DateTime.UtcNow;
            employee.TerminationReason = reason;
            employee.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Employee deactivated: {EmpId}", employee.Emp_ID);
            return true;
        }

        public async Task<bool> ActivateEmployeeAsync(int id)
        {
            _logger.LogInformation("Activating employee ID: {EmployeeId}", id);

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return false;

            employee.IsActive = true;
            employee.TerminationDate = null;
            employee.TerminationReason = null;
            employee.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Employee activated: {EmpId}", employee.Emp_ID);
            return true;
        }

        public async Task<string> GenerateEmployeeIdAsync()
        {
            var lastEmployee = await _context.Employees
                .OrderByDescending(e => e.Id)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastEmployee != null)
            {
                var lastId = lastEmployee.Emp_ID;
                if (int.TryParse(lastId.Substring(3), out int num))
                {
                    nextNumber = num + 1;
                }
            }

            return $"EMP{nextNumber:D5}";
        }

        public async Task<IEnumerable<EmployeeListViewModel>> GetEmployeesOnProbationAsync()
        {
            _logger.LogInformation("Fetching employees on probation");
            return await _context.Employees
                .Where(e => e.IsActive && e.ProbationStatus == ProbationStatus.Active)
                .Select(e => new EmployeeListViewModel
                {
                    Id = e.Id,
                    Emp_ID = e.Emp_ID,
                    FullName = e.FullName,
                    Designation = e.Designation,
                    Department = e.Department,
                    EmploymentType = e.EmploymentType.ToString(),
                    ProbationStatus = e.ProbationStatus.ToString(),
                    IsActive = e.IsActive,
                    Join_Date = e.Join_Date
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ProbationExpiryViewModel>> GetUpcomingProbationExpiriesAsync(int daysAhead = 30)
        {
            _logger.LogInformation("Fetching upcoming probation expiries for next {Days} days", daysAhead);
            var targetDate = DateTime.UtcNow.AddDays(daysAhead);

            return await _context.Employees
                .Where(e => e.IsActive && e.ProbationStatus == ProbationStatus.Active && e.Probation_End <= targetDate)
                .Select(e => new ProbationExpiryViewModel
                {
                    EmployeeId = e.Id,
                    Emp_ID = e.Emp_ID,
                    EmployeeName = e.FullName,
                    ProbationEndDate = e.Probation_End!.Value,
                    DaysRemaining = EF.Functions.DateDiffDay(DateTime.UtcNow, e.Probation_End.Value)
                })
                .OrderBy(e => e.DaysRemaining)
                .ToListAsync();
        }

        public async Task<bool> CompleteProbationAsync(int employeeId)
        {
            _logger.LogInformation("Completing probation for employee ID: {EmployeeId}", employeeId);

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null) return false;

            employee.ProbationStatus = ProbationStatus.Completed;
            employee.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Probation completed for employee: {EmpId}", employee.Emp_ID);
            return true;
        }

        public async Task<bool> ExtendProbationAsync(int employeeId, DateTime newEndDate)
        {
            _logger.LogInformation("Extending probation for employee ID: {EmployeeId} to {NewEndDate}", employeeId, newEndDate);

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null) return false;

            employee.ProbationStatus = ProbationStatus.Extended;
            employee.Probation_End = newEndDate;
            employee.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Probation extended for employee: {EmpId}", employee.Emp_ID);
            return true;
        }

        public async Task<int> GetTotalActiveEmployeesCountAsync()
        {
            return await _context.Employees.CountAsync(e => e.IsActive);
        }

        public async Task<int> GetEmployeesOnProbationCountAsync()
        {
            return await _context.Employees.CountAsync(e => e.IsActive && e.ProbationStatus == ProbationStatus.Active);
        }

        public async Task<IEnumerable<EmployeeListViewModel>> SearchEmployeesAsync(string searchTerm)
        {
            _logger.LogInformation("Searching employees with term: {SearchTerm}", searchTerm);
            var lowerTerm = searchTerm.ToLower();

            return await _context.Employees
                .Where(e => e.IsActive && (
                    e.FirstName.ToLower().Contains(lowerTerm) ||
                    e.LastName.ToLower().Contains(lowerTerm) ||
                    e.Emp_ID.ToLower().Contains(lowerTerm) ||
                    e.Department.ToLower().Contains(lowerTerm) ||
                    e.Designation.ToLower().Contains(lowerTerm) ||
                    (e.PAN_No != null && e.PAN_No.ToLower().Contains(lowerTerm))
                ))
                .Select(e => new EmployeeListViewModel
                {
                    Id = e.Id,
                    Emp_ID = e.Emp_ID,
                    FullName = e.FullName,
                    Designation = e.Designation,
                    Department = e.Department,
                    EmploymentType = e.EmploymentType.ToString(),
                    ProbationStatus = e.ProbationStatus.ToString(),
                    IsActive = e.IsActive,
                    Join_Date = e.Join_Date
                })
                .Take(50)
                .ToListAsync();
        }

        private async Task InitializeLeaveBalanceForEmployee(int employeeId, int year)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null) return;

            var existingBalance = await _context.LeaveBalances
                .AnyAsync(lb => lb.EmployeeId == employeeId && lb.Year == year);

            if (!existingBalance)
            {
                var leaveBalance = new LeaveBalance
                {
                    EmployeeId = employeeId,
                    Year = year,
                    Home_Leave_Accrued = 0,
                    Home_Leave_Used = 0,
                    Sick_Leave_Total = 12,
                    Sick_Leave_Taken = 0,
                    Maternity_Leave_Total = employee.Gender == Gender.Female ? 98 : 0,
                    Maternity_Leave_Used = 0,
                    Maternity_Status = employee.Gender == Gender.Female ? "Eligible" : "NotApplicable",
                    Paternity_Leave_Total = employee.Gender == Gender.Male ? 15 : 0,
                    Paternity_Leave_Used = 0,
                    Mourning_Leave_Total = 13,
                    Mourning_Leave_Used = 0,
                    Public_Holidays_Total = employee.Gender == Gender.Female ? 14 : 13,
                    Public_Holidays_Used = 0,
                    CreatedAt = DateTime.UtcNow
                };

                _context.LeaveBalances.Add(leaveBalance);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Leave balance initialized for employee ID: {EmployeeId}, Year: {Year}", employeeId, year);
            }
        }

        public async Task<Employee?> GetEmployeeByUserIdAsync(string userId)
        {
            _logger.LogInformation("Fetching employee by user ID: {UserId}", userId);
            return await _context.Employees.FirstOrDefaultAsync(e => e.Email == userId || e.Emp_ID == userId);
        }
    }
}
