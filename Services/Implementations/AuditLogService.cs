using Microsoft.EntityFrameworkCore;
using HR_Management_System.Data;
using HR_Management_System.Models;
using HR_Management_System.Models.ViewModels;
using HR_Management_System.Services.Interfaces;

namespace HR_Management_System.Services.Implementations
{
    public class AuditLogService : IAuditLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(ApplicationDbContext context, ILogger<AuditLogService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<AuditLogListViewModel> GetAuditLogsAsync(AuditLogFilterViewModel filter)
        {
            _logger.LogInformation("Fetching audit logs with filter");

            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(filter.Level))
                query = query.Where(l => l.Level == filter.Level);

            if (filter.FromDate.HasValue)
                query = query.Where(l => l.TimeStamp >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(l => l.TimeStamp <= filter.ToDate.Value);

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(l =>
                    (l.Message != null && l.Message.ToLower().Contains(searchTerm)) ||
                    (l.SourceContext != null && l.SourceContext.ToLower().Contains(searchTerm)) ||
                    (l.UserName != null && l.UserName.ToLower().Contains(searchTerm)) ||
                    (l.Action != null && l.Action.ToLower().Contains(searchTerm))
                );
            }

            var totalCount = await query.CountAsync();

            var logs = await query
                .OrderByDescending(l => l.TimeStamp)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new AuditLogListViewModel
            {
                Logs = logs,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<AuditLog?> GetAuditLogByIdAsync(int id)
        {
            return await _context.AuditLogs.FindAsync(id);
        }

        public async Task<IEnumerable<AuditLog>> GetRecentLogsAsync(int count = 100)
        {
            return await _context.AuditLogs
                .OrderByDescending(l => l.TimeStamp)
                .Take(count)
                .ToListAsync();
        }
    }
}