using HR_Management_System.Models;
using HR_Management_System.Models.ViewModels;

namespace HR_Management_System.Services.Interfaces
{
    public interface ITeamService
    {
        // Team operations
        Task<IEnumerable<TeamListViewModel>> GetAllTeamsAsync();
        Task<TeamDetailViewModel?> GetTeamByIdAsync(int id);
        Task<Team> CreateTeamAsync(TeamCreateViewModel model, int createdByUserId);
        Task<Team> UpdateTeamAsync(TeamEditViewModel model);
        Task<bool> DeactivateTeamAsync(int id);
        Task<bool> ActivateTeamAsync(int id);
        Task<bool> DeleteTeamAsync(int id);
        
        // Team member operations
        Task<bool> AddTeamMemberAsync(int teamId, int employeeId, string role = "Member");
        Task<bool> RemoveTeamMemberAsync(int teamId, int employeeId);
        Task<bool> UpdateTeamMemberRoleAsync(int teamId, int employeeId, string role);
        Task<IEnumerable<TeamMemberViewModel>> GetTeamMembersAsync(int teamId);
        
        // Project task operations
        Task<IEnumerable<ProjectTaskViewModel>> GetTeamTasksAsync(int teamId);
        Task<ProjectTaskViewModel?> GetTaskByIdAsync(int taskId);
        Task<ProjectTask> CreateTaskAsync(ProjectTaskCreateViewModel model, int assignedByUserId);
        Task<ProjectTask> UpdateTaskAsync(ProjectTaskEditViewModel model);
        Task<bool> DeleteTaskAsync(int taskId);
        Task<bool> UpdateTaskStatusAsync(int taskId, string status);
        
        // Employee team operations
        Task<IEnumerable<TeamListViewModel>> GetEmployeeTeamsAsync(int employeeId);
        Task<IEnumerable<ProjectTaskViewModel>> GetEmployeeTasksAsync(int employeeId);
        
        // Notification operations
        Task SendTeamCreationNotificationAsync(int teamId, int createdByUserId);
        Task SendTaskAssignmentNotificationAsync(int taskId, int assignedByUserId);
        
        // Utility methods
        Task<Dictionary<int, string>> GetAvailableDepartmentsAsync();
        Task<Dictionary<int, string>> GetAvailableEmployeesAsync();
        Task<int> GetTeamMemberCountAsync(int teamId);
        Task<int> GetTeamTaskCountAsync(int teamId);

        // Task extension request operations
        Task<TaskExtensionRequest> CreateExtensionRequestAsync(int taskId, int requestedByEmployeeId, DateTime requestedDeadline, string reason);
        Task<IEnumerable<TaskExtensionRequest>> GetExtensionRequestsByTaskAsync(int taskId);
        Task<IEnumerable<TaskExtensionRequest>> GetExtensionRequestsByEmployeeAsync(int employeeId);
        Task<TaskExtensionRequest?> GetExtensionRequestByIdAsync(int requestId);
        Task<TaskExtensionRequest> ApproveExtensionRequestAsync(int requestId, int reviewedByEmployeeId, string? comments = null);
        Task<TaskExtensionRequest> RejectExtensionRequestAsync(int requestId, int reviewedByEmployeeId, string? comments = null);
        Task<bool> ApplyPerformanceDeductionAsync(int requestId);
    }
}