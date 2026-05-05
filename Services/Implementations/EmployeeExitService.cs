using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HR_Management_System.Data;
using HR_Management_System.Models;
using HR_Management_System.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HR_Management_System.Services.Implementations
{
    public class EmployeeExitService : IEmployeeExitService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EmployeeExitService> _logger;
        private readonly IEmployeeService _employeeService;
        private readonly IPayrollService _payrollService;

        public EmployeeExitService(
            ApplicationDbContext context,
            ILogger<EmployeeExitService> logger,
            IEmployeeService employeeService,
            IPayrollService payrollService)
        {
            _context = context;
            _logger = logger;
            _employeeService = employeeService;
            _payrollService = payrollService;
        }

        // Employee Exit Process
        public async Task<EmployeeExit> InitiateExitProcessAsync(EmployeeExit exit)
        {
            try
            {
                // Validate employee exists and is active
                var employee = await _context.Employees.FindAsync(exit.EmployeeId);
                if (employee == null)
                    throw new ArgumentException($"Employee with ID {exit.EmployeeId} not found.");

                if (employee.EmploymentStatus != "Active")
                    throw new InvalidOperationException($"Employee is not active. Current status: {employee.EmploymentStatus}");

                // Set default values
                exit.ExitDate = DateTime.UtcNow;
                exit.Status = ExitStatus.Pending;
                exit.CreatedAt = DateTime.UtcNow;
                exit.UpdatedAt = DateTime.UtcNow;

                // Calculate notice period based on Nepal labor law
                await CalculateNoticePeriodAsync(exit.Id);

                _context.EmployeeExits.Add(exit);
                await _context.SaveChangesAsync();

                // Create default clearance items
                await CreateDefaultExitClearancesAsync(exit.Id);

                _logger.LogInformation($"Exit process initiated for employee {employee.Emp_ID} (ID: {exit.Id})");
                return exit;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error initiating exit process for employee {exit.EmployeeId}");
                throw;
            }
        }

        public async Task<EmployeeExit> GetExitProcessByIdAsync(int id)
        {
            return await _context.EmployeeExits
                .Include(e => e.Employee)
                .Include(e => e.ExitClearances)
                .Include(e => e.ExitDocuments)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<EmployeeExit>> GetAllExitProcessesAsync(string status = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.EmployeeExits
                .Include(e => e.Employee)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<ExitStatus>(status, out var statusEnum))
                {
                    query = query.Where(e => e.Status == statusEnum);
                }
            }

            if (startDate.HasValue)
                query = query.Where(e => e.ExitDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.ExitDate <= endDate.Value);

            return await query.OrderByDescending(e => e.ExitDate).ToListAsync();
        }

        public async Task<EmployeeExit> UpdateExitProcessAsync(int id, EmployeeExit exit)
        {
            var existingExit = await _context.EmployeeExits.FindAsync(id);
            if (existingExit == null)
                throw new ArgumentException($"Exit process with ID {id} not found.");

            // Update only allowed fields
            existingExit.ExitType = exit.ExitType;
            existingExit.Reason = exit.Reason;
            existingExit.LastWorkingDay = exit.LastWorkingDay;
            existingExit.NoticePeriodDays = exit.NoticePeriodDays;
            existingExit.IsNoticePeriodWaived = exit.IsNoticePeriodWaived;
            existingExit.NoticePeriodWaivedBy = exit.NoticePeriodWaivedBy;
            existingExit.NoticePeriodWaivedReason = exit.NoticePeriodWaivedReason;
            existingExit.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingExit;
        }

        public async Task<bool> DeleteExitProcessAsync(int id)
        {
            var exit = await _context.EmployeeExits.FindAsync(id);
            if (exit == null) return false;

            if (exit.Status == ExitStatus.Approved || exit.Status == ExitStatus.Completed)
                throw new InvalidOperationException("Cannot delete approved or completed exit process.");

            _context.EmployeeExits.Remove(exit);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ApproveExitProcessAsync(int id, string approvedBy, string comments = null)
        {
            var exit = await _context.EmployeeExits.FindAsync(id);
            if (exit == null) return false;

            exit.Status = ExitStatus.Approved;
            exit.ApprovedBy = approvedBy;
            exit.ApprovalDate = DateTime.UtcNow;
            exit.ApprovalComments = comments;
            exit.UpdatedAt = DateTime.UtcNow;

            // Update employee status
            var employee = await _context.Employees.FindAsync(exit.EmployeeId);
            if (employee != null)
            {
                employee.EmploymentStatus = "Resigned";
                employee.ResignationDate = exit.ExitDate;
                employee.LastWorkingDay = exit.LastWorkingDay;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectExitProcessAsync(int id, string rejectedBy, string reason)
        {
            var exit = await _context.EmployeeExits.FindAsync(id);
            if (exit == null) return false;

            exit.Status = ExitStatus.Rejected;
            exit.RejectedBy = rejectedBy;
            exit.RejectionDate = DateTime.UtcNow;
            exit.RejectionReason = reason;
            exit.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelExitProcessAsync(int id, string reason)
        {
            var exit = await _context.EmployeeExits.FindAsync(id);
            if (exit == null) return false;

            exit.Status = ExitStatus.Cancelled;
            exit.CancellationReason = reason;
            exit.CancellationDate = DateTime.UtcNow;
            exit.UpdatedAt = DateTime.UtcNow;

            // Revert employee status if needed
            var employee = await _context.Employees.FindAsync(exit.EmployeeId);
            if (employee != null && employee.EmploymentStatus == "Resigned")
            {
                employee.EmploymentStatus = "Active";
                employee.ResignationDate = null;
                employee.LastWorkingDay = null;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // Exit Clearance Management
        public async Task<ExitClearance> CreateExitClearanceAsync(ExitClearance clearance)
        {
            _context.ExitClearances.Add(clearance);
            await _context.SaveChangesAsync();
            return clearance;
        }

        public async Task<ExitClearance> GetExitClearanceByIdAsync(int id)
        {
            return await _context.ExitClearances
                .Include(c => c.EmployeeExit)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<ExitClearance>> GetExitClearancesByExitIdAsync(int exitId)
        {
            return await _context.ExitClearances
                .Where(c => c.ExitId == exitId)
                .OrderBy(c => c.Department)
                .ToListAsync();
        }

        public async Task<ExitClearance> UpdateExitClearanceAsync(int id, ExitClearance clearance)
        {
            var existing = await _context.ExitClearances.FindAsync(id);
            if (existing == null)
                throw new ArgumentException($"Clearance with ID {id} not found.");

            existing.Department = clearance.Department;
            existing.ClearanceItem = clearance.ClearanceItem;
            existing.ResponsiblePerson = clearance.ResponsiblePerson;
            existing.Status = clearance.Status;
            existing.Remarks = clearance.Remarks;
            existing.ClearedDate = clearance.ClearedDate;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteExitClearanceAsync(int id)
        {
            var clearance = await _context.ExitClearances.FindAsync(id);
            if (clearance == null) return false;

            _context.ExitClearances.Remove(clearance);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CompleteExitClearanceAsync(int clearanceId, string clearedBy, string remarks = null)
        {
            var clearance = await _context.ExitClearances.FindAsync(clearanceId);
            if (clearance == null) return false;

            clearance.Status = ClearanceStatus.Cleared;
            clearance.ClearedBy = clearedBy;
            clearance.ClearedDate = DateTime.UtcNow;
            clearance.Remarks = remarks;
            clearance.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> WaiveExitClearanceAsync(int clearanceId, string waivedBy, string reason)
        {
            var clearance = await _context.ExitClearances.FindAsync(clearanceId);
            if (clearance == null) return false;

            clearance.Status = ClearanceStatus.Waived;
            clearance.WaivedBy = waivedBy;
            clearance.WaivedDate = DateTime.UtcNow;
            clearance.WaivedReason = reason;
            clearance.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<object> GetExitClearanceStatusAsync(int exitId)
        {
            var clearances = await GetExitClearancesByExitIdAsync(exitId);
            var total = clearances.Count();
            var cleared = clearances.Count(c => c.Status == ClearanceStatus.Cleared);
            var pending = clearances.Count(c => c.Status == ClearanceStatus.Pending);
            var waived = clearances.Count(c => c.Status == ClearanceStatus.Waived);

            return new
            {
                Total = total,
                Cleared = cleared,
                Pending = pending,
                Waived = waived,
                CompletionPercentage = total > 0 ? (cleared + waived) * 100 / total : 0,
                Clearances = clearances.Select(c => new
                {
                    c.Id,
                    c.Department,
                    c.ClearanceItem,
                    c.Status,
                    c.ResponsiblePerson,
                    c.ClearedDate,
                    c.Remarks
                })
            };
        }

        // Exit Survey Management
        public async Task<ExitSurvey> CreateExitSurveyAsync(ExitSurvey survey)
        {
            _context.ExitSurveys.Add(survey);
            await _context.SaveChangesAsync();
            return survey;
        }

        public async Task<ExitSurvey> GetExitSurveyByExitIdAsync(int exitId)
        {
            return await _context.ExitSurveys
                .FirstOrDefaultAsync(s => s.ExitId == exitId);
        }

        public async Task<ExitSurvey> UpdateExitSurveyAsync(int id, ExitSurvey survey)
        {
            var existing = await _context.ExitSurveys.FindAsync(id);
            if (existing == null)
                throw new ArgumentException($"Survey with ID {id} not found.");

            existing.ReasonForLeaving = survey.ReasonForLeaving;
            existing.SatisfactionRating = survey.SatisfactionRating;
            existing.WouldRecommend = survey.WouldRecommend;
            existing.Comments = survey.Comments;
            existing.Suggestions = survey.Suggestions;
            existing.SubmittedDate = survey.SubmittedDate;
            existing.IsAnonymous = survey.IsAnonymous;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteExitSurveyAsync(int id)
        {
            var survey = await _context.ExitSurveys.FindAsync(id);
            if (survey == null) return false;

            _context.ExitSurveys.Remove(survey);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SubmitExitSurveyAsync(int exitId, ExitSurvey survey)
        {
            survey.ExitId = exitId;
            survey.SubmittedDate = DateTime.UtcNow;
            survey.CreatedAt = DateTime.UtcNow;

            _context.ExitSurveys.Add(survey);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<object> GetExitSurveyAnalyticsAsync(int? year = null, string department = null)
        {
            var query = _context.ExitSurveys
                .Include(s => s.EmployeeExit)
                .ThenInclude(e => e.Employee)
                .AsQueryable();

            if (year.HasValue)
            {
                var startDate = new DateTime(year.Value, 1, 1);
                var endDate = new DateTime(year.Value, 12, 31);
                query = query.Where(s => s.SubmittedDate >= startDate && s.SubmittedDate <= endDate);
            }

            if (!string.IsNullOrEmpty(department))
            {
                query = query.Where(s => s.EmployeeExit.Employee.Department == department);
            }

            var surveys = await query.ToListAsync();

            if (!surveys.Any())
                return new { Message = "No exit survey data available." };

            var averageSatisfaction = surveys.Average(s => s.SatisfactionRating);
            var recommendationRate = surveys.Count(s => s.WouldRecommend) * 100 / surveys.Count;

            var reasons = surveys
                .GroupBy(s => s.ReasonForLeaving)
                .Select(g => new
                {
                    Reason = g.Key,
                    Count = g.Count(),
                    Percentage = g.Count() * 100 / surveys.Count
                })
                .OrderByDescending(r => r.Count)
                .ToList();

            return new
            {
                TotalSurveys = surveys.Count,
                AverageSatisfaction = Math.Round(averageSatisfaction, 2),
                RecommendationRate = recommendationRate,
                TopReasons = reasons.Take(5),
                DepartmentBreakdown = surveys
                    .GroupBy(s => s.EmployeeExit.Employee.Department)
                    .Select(g => new
                    {
                        Department = g.Key,
                        Count = g.Count(),
                        AvgSatisfaction = Math.Round(g.Average(s => s.SatisfactionRating), 2)
                    })
                    .OrderByDescending(d => d.Count)
                    .ToList()
            };
        }

        // Exit Document Management
        public async Task<ExitDocument> CreateExitDocumentAsync(ExitDocument document)
        {
            _context.ExitDocuments.Add(document);
            await _context.SaveChangesAsync();
            return document;
        }

        public async Task<ExitDocument> GetExitDocumentByIdAsync(int id)
        {
            return await _context.ExitDocuments
                .Include(d => d.EmployeeExit)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<ExitDocument>> GetExitDocumentsByExitIdAsync(int exitId)
        {
            return await _context.ExitDocuments
                .Where(d => d.ExitId == exitId)
                .OrderBy(d => d.DocumentType)
                .ToListAsync();
        }

        public async Task<ExitDocument> UpdateExitDocumentAsync(int id, ExitDocument document)
        {
            var existing = await _context.ExitDocuments.FindAsync(id);
            if (existing == null)
                throw new ArgumentException($"Document with ID {id} not found.");

            existing.DocumentType = document.DocumentType;
            existing.FilePath = document.FilePath;
            existing.FileName = document.FileName;
            existing.Description = document.Description;
            existing.UploadedBy = document.UploadedBy;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteExitDocumentAsync(int id)
        {
            var document = await _context.ExitDocuments.FindAsync(id);
            if (document == null) return false;

            _context.ExitDocuments.Remove(document);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UploadExitDocumentAsync(int exitId, string documentType, string filePath, string fileName, string description = null)
        {
            var document = new ExitDocument
            {
                ExitId = exitId,
                DocumentType = documentType,
                FilePath = filePath,
                FileName = fileName,
                Description = description,
                UploadedBy = "System",
                UploadedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ExitDocuments.Add(document);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<object> DownloadExitDocumentAsync(int documentId)
        {
            var document = await _context.ExitDocuments.FindAsync(documentId);
            if (document == null)
                throw new ArgumentException($"Document with ID {documentId} not found.");

            // In a real implementation, this would read the file from storage
            // For now, return document metadata
            return new
            {
                document.Id,
                document.FileName,
                document.FilePath,
                document.DocumentType,
                document.Description,
                document.UploadedDate,
                DownloadUrl = $"/api/exit/documents/{documentId}/download"
            };
        }

        // Full & Final Settlement
        public async Task<object> CalculateFullAndFinalSettlementAsync(int exitId)
        {
            var exit = await GetExitProcessByIdAsync(exitId);
            if (exit == null)
                throw new ArgumentException($"Exit process with ID {exitId} not found.");

            var employee = await _context.Employees.FindAsync(exit.EmployeeId);
            if (employee == null)
                throw new ArgumentException($"Employee not found for exit process {exitId}");

            // Get last payroll
            var lastPayroll = await _context.Payrolls
                .Where(p => p.EmployeeId == exit.EmployeeId)
                .OrderByDescending(p => p.PayPeriodEnd)
                .FirstOrDefaultAsync();

            // Calculate components based on Nepal labor law
            var basicSalary = employee.BaseSalary;
            var daysWorkedInMonth = CalculateDaysWorkedInMonth(exit.LastWorkingDay ?? DateTime.UtcNow);
            var proratedSalary = basicSalary * daysWorkedInS