using Microsoft.EntityFrameworkCore;
using HR_Management_System.Data;
using HR_Management_System.Models;
using HR_Management_System.Models.ViewModels;
using HR_Management_System.Services.Interfaces;

namespace HR_Management_System.Services.Implementations
{
    public class SSFService : ISSFService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SSFService> _logger;

        public SSFService(ApplicationDbContext context, ILogger<SSFService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<SSFBalanceViewModel> GetSSFBalanceAsync(int employeeId)
        {
            _logger.LogInformation("Fetching SSF balance for employee ID: {EmployeeId}", employeeId);

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
                throw new KeyNotFoundException("Employee not found");

            var latestRecord = await _context.SSFRecords
                .Where(s => s.EmployeeId == employeeId)
                .OrderByDescending(s => s.Year).ThenByDescending(s => s.Month)
                .FirstOrDefaultAsync();

            var totalEmployeeContribution = await _context.SSFRecords
                .Where(s => s.EmployeeId == employeeId)
                .SumAsync(s => s.EmployeeContribution);

            var totalEmployerContribution = await _context.SSFRecords
                .Where(s => s.EmployeeId == employeeId)
                .SumAsync(s => s.EmployerContribution);

            return new SSFBalanceViewModel
            {
                EmployeeId = employeeId,
                EmployeeName = employee.FullName,
                TotalEmployeeContribution = totalEmployeeContribution,
                TotalEmployerContribution = totalEmployerContribution,
                TotalBalance = totalEmployeeContribution + totalEmployerContribution,
                LastContributionDate = latestRecord?.CreatedAt.Ticks ?? 0
            };
        }

        public async Task<IEnumerable<SSFRecord>> GetSSFHistoryAsync(int employeeId)
        {
            return await _context.SSFRecords
                .Where(s => s.EmployeeId == employeeId)
                .OrderByDescending(s => s.Year).ThenByDescending(s => s.Month)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalEmployeeContributionAsync(int employeeId)
        {
            return await _context.SSFRecords
                .Where(s => s.EmployeeId == employeeId)
                .SumAsync(s => s.EmployeeContribution);
        }

        public async Task<decimal> GetTotalEmployerContributionAsync(int employeeId)
        {
            return await _context.SSFRecords
                .Where(s => s.EmployeeId == employeeId)
                .SumAsync(s => s.EmployerContribution);
        }
    }
}