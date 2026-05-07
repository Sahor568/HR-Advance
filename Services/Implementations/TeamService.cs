using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HR_Management_System.Data;
using HR_Management_System.Models;
using HR_Management_System.Models.ViewModels;
using HR_Management_System.Services.Interfaces;

namespace HR_Management_System.Services.Implementations
{
    public class TeamService : ITeamService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TeamService> _logger;
        private readonly IHRISService _hrisService;

        public TeamService(ApplicationDbContext context, ILogger<TeamService> logger, IHRISService hrisService)
        {
            _context = context;
            _logger = logger;
            _hrisService = hrisService;
        }

        public async Task<IEnumerable<TeamListViewModel>> GetAllTeamsAsync()
        {
            _logger.LogInformation("Fetching all teams");
            return await _context.Teams
                .Include(t => t.Department)
                .Include(t => t.Creator)
                .Include(t => t.TeamMembers)
                .Include(t => t.Tasks)
                .Select(t => new TeamListViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    DepartmentName = t.Department != null ? t.Department.Name : "N/A",
                    CreatorName = t.Creator != null ? t.Creator.FullName : "Unknown",
                    CreatedAt = t.CreatedAt,
                    MemberCount = t.TeamMembers.Count(m => m.IsActive),
                    TaskCount = t.Tasks.Count,
                    IsActive = t.IsActive
                })
                .ToListAsync();
        }

        public async Task<TeamDetailViewModel?> GetTeamByIdAsync(int id)
        {
            _logger.LogInformation($"Fetching team with ID {id}");
            var team = await _context.Teams
                .Include(t => t.Department)
                .Include(t => t.Creator)
                .Include(t => t.TeamMembers)
                    .ThenInclude(tm => tm.Employee)
                .Include(t => t.Tasks)
                    .ThenInclude(task => task.AssignedEmployee)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (team == null)
                return null;

            return new TeamDetailViewModel
            {
                Id = team.Id,
                Name = team.Name,
                Description = team.Description,
                DepartmentId = team.DepartmentId,
                DepartmentName = team.Department != null ? team.Department.Name : "N/A",
                CreatedBy = team.CreatedBy,
                CreatorName = team.Creator != null ? team.Creator.FullName : "Unknown",
                CreatedAt = team.CreatedAt,
                UpdatedAt = team.UpdatedAt,
                IsActive = team.IsActive,
                Members = team.TeamMembers
                    .Where(tm => tm.IsActive)
                    .Select(tm => new TeamMemberViewModel
                    {
                        Id = tm.Id,
                        EmployeeId = tm.EmployeeId,
                        EmployeeName = tm.Employee != null ? tm.Employee.FullName : "Unknown",
                        EmployeeCode = tm.Employee != null ? tm.Employee.Emp_ID : "N/A",
                        Designation = tm.Employee != null ? tm.Employee.Designation : "N/A",
                        Role = tm.Role,
                        JoinedAt = tm.JoinedAt,
                        LeftAt = tm.LeftAt,
                        IsActive = tm.IsActive
                    }).ToList(),
                Tasks = team.Tasks.Select(t => new ProjectTaskViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    TeamId = t.TeamId,
                    TeamName = team.Name,
                    AssignedTo = t.AssignedTo,
                    AssignedEmployeeName = t.AssignedEmployee != null ? t.AssignedEmployee.FullName : "Unassigned",
                    StartDate = t.StartDate,
                    Deadline = t.Deadline,
                    Status = t.Status,
                    Priority = t.Priority,
                    CreatedAt = t.StartDate
                }).ToList()
            };
        }

        public async Task<Team> CreateTeamAsync(TeamCreateViewModel model, int createdByUserId)
        {
            _logger.LogInformation($"Creating new team: {model.Name}");
            
            // Create team
            var team = new Team
            {
                Name = model.Name,
                Description = model.Description,
                DepartmentId = model.DepartmentId,
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            // Add team members
            foreach (var employeeId in model.MemberIds)
            {
                var teamMember = new TeamMember
                {
                    TeamId = team.Id,
                    EmployeeId = employeeId,
                    Role = "Member",
                    JoinedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.TeamMembers.Add(teamMember);
            }

            await _context.SaveChangesAsync();
            
            // Send notifications
            await SendTeamCreationNotificationAsync(team.Id, createdByUserId);
            
            _logger.LogInformation($"Team created successfully with ID: {team.Id}");
            return team;
        }

        public async Task<Team> UpdateTeamAsync(TeamEditViewModel model)
        {
            _logger.LogInformation($"Updating team with ID {model.Id}");
            
            var team = await _context.Teams.FindAsync(model.Id);
            if (team == null)
                throw new ArgumentException($"Team with ID {model.Id} not found");

            team.Name = model.Name;
            team.Description = model.Description;
            team.DepartmentId = model.DepartmentId;
            team.UpdatedAt = DateTime.UtcNow;

            // Update team members
            var existingMembers = await _context.TeamMembers
                .Where(tm => tm.TeamId == model.Id && tm.IsActive)
                .ToListAsync();

            // Remove members not in new list
            foreach (var existingMember in existingMembers)
            {
                if (!model.MemberIds.Contains(existingMember.EmployeeId))
                {
                    existingMember.IsActive = false;
                    existingMember.LeftAt = DateTime.UtcNow;
                }
            }

            // Add new members
            foreach (var employeeId in model.MemberIds)
            {
                if (!existingMembers.Any(em => em.EmployeeId == employeeId && em.IsActive))
                {
                    var teamMember = new TeamMember
                    {
                        TeamId = model.Id,
                        EmployeeId = employeeId,
                        Role = "Member",
                        JoinedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    _context.TeamMembers.Add(teamMember);
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Team with ID {model.Id} updated successfully");
            return team;
        }

        public async Task<bool> DeactivateTeamAsync(int id)
        {
            _logger.LogInformation($"Deactivating team with ID {id}");
            
            var team = await _context.Teams.FindAsync(id);
            if (team == null)
                return false;

            team.IsActive = false;
            team.UpdatedAt = DateTime.UtcNow;
            
            // Deactivate all team members
            var teamMembers = await _context.TeamMembers
                .Where(tm => tm.TeamId == id && tm.IsActive)
                .ToListAsync();
            
            foreach (var member in teamMembers)
            {
                member.IsActive = false;
                member.LeftAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Team with ID {id} deactivated successfully");
            return true;
        }

        public async Task<bool> ActivateTeamAsync(int id)
        {
            _logger.LogInformation($"Activating team with ID {id}");
            
            var team = await _context.Teams.FindAsync(id);
            if (team == null)
                return false;

            team.IsActive = true;
            team.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Team with ID {id} activated successfully");
            return true;
        }

        public async Task<bool> DeleteTeamAsync(int id)
        {
            _logger.LogInformation($"Deleting team with ID {id}");
            
            var team = await _context.Teams
                .Include(t => t.TeamMembers)
                .Include(t => t.Tasks)
                .FirstOrDefaultAsync(t => t.Id == id);
            
            if (team == null)
                return false;

            // Remove team members
            _context.TeamMembers.RemoveRange(team.TeamMembers);
            
            // Remove tasks
            _context.ProjectTasks.RemoveRange(team.Tasks);
            
            // Remove team
            _context.Teams.Remove(team);
            
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Team with ID {id} deleted successfully");
            return true;
        }

        public async Task<bool> AddTeamMemberAsync(int teamId, int employeeId, string role = "Member")
        {
            _logger.LogInformation($"Adding employee {employeeId} to team {teamId}");
            
            // Check if already a member
            var existingMember = await _context.TeamMembers
                .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.EmployeeId == employeeId && tm.IsActive);
            
            if (existingMember != null)
                return false; // Already a member

            var teamMember = new TeamMember
            {
                TeamId = teamId,
                EmployeeId = employeeId,
                Role = role,
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.TeamMembers.Add(teamMember);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Employee {employeeId} added to team {teamId} successfully");
            return true;
        }

        public async Task<bool> RemoveTeamMemberAsync(int teamId, int employeeId)
        {
            _logger.LogInformation($"Removing employee {employeeId} from team {teamId}");
            
            var teamMember = await _context.TeamMembers
                .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.EmployeeId == employeeId && tm.IsActive);
            
            if (teamMember == null)
                return false;

            teamMember.IsActive = false;
            teamMember.LeftAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Employee {employeeId} removed from team {teamId} successfully");
            return true;
        }

        public async Task<bool> UpdateTeamMemberRoleAsync(int teamId, int employeeId, string role)
        {
            _logger.LogInformation($"Updating role for employee {employeeId} in team {teamId}");
            
            var teamMember = await _context.TeamMembers
                .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.EmployeeId == employeeId && tm.IsActive);
            
            if (teamMember == null)
                return false;

            teamMember.Role = role;
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Role updated for employee {employeeId} in team {teamId}");
            return true;
        }

        public async Task<IEnumerable<TeamMemberViewModel>> GetTeamMembersAsync(int teamId)
        {
            _logger.LogInformation($"Fetching members for team {teamId}");
            
            return await _context.TeamMembers
                .Include(tm => tm.Employee)
                .Where(tm => tm.TeamId == teamId && tm.IsActive)
                .Select(tm => new TeamMemberViewModel
                {
                    Id = tm.Id,
                    EmployeeId = tm.EmployeeId,
                    EmployeeName = tm.Employee != null ? tm.Employee.FullName : "Unknown",
                    EmployeeCode = tm.Employee != null ? tm.Employee.Emp_ID : "N/A",
                    Designation = tm.Employee != null ? tm.Employee.Designation : "N/A",
                    Role = tm.Role,
                    JoinedAt = tm.JoinedAt,
                    LeftAt = tm.LeftAt,
                    IsActive = tm.IsActive
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ProjectTaskViewModel>> GetTeamTasksAsync(int teamId)
        {
            _logger.LogInformation($"Fetching tasks for team {teamId}");
            
            return await _context.ProjectTasks
                .Include(t => t.AssignedEmployee)
                .Where(t => t.TeamId == teamId)
                .Select(t => new ProjectTaskViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    TeamId = t.TeamId,
                    TeamName = t.Team != null ? t.Team.Name : "Unknown",
                    AssignedTo = t.AssignedTo,
                    AssignedEmployeeName = t.AssignedEmployee != null ? t.AssignedEmployee.FullName : "Unassigned",
                    StartDate = t.StartDate,
                    Deadline = t.Deadline,
                    Status = t.Status,
                    Priority = t.Priority,
                    CreatedAt = t.StartDate
                })
                .ToListAsync();
        }

        public async Task<ProjectTaskViewModel?> GetTaskByIdAsync(int taskId)
        {
            _logger.LogInformation($"Fetching task with ID {taskId}");
            
            var task = await _context.ProjectTasks
                .Include(t => t.Team)
                .Include(t => t.AssignedEmployee)
                .FirstOrDefaultAsync(t => t.Id == taskId);
            
            if (task == null)
                return null;

            return new ProjectTaskViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                TeamId = task.TeamId,
                TeamName = task.Team != null ? task.Team.Name : "Unknown",
                AssignedTo = task.AssignedTo,
                AssignedEmployeeName = task.AssignedEmployee != null ? task.AssignedEmployee.FullName : "Unassigned",
                StartDate = task.StartDate,
                Deadline = task.Deadline,
                Status = task.Status,
                Priority = task.Priority,
                CreatedAt = task.StartDate
            };
        }

        public async Task<ProjectTask> CreateTaskAsync(ProjectTaskCreateViewModel model, int assignedByUserId)
        {
            _logger.LogInformation($"Creating new task: {model.Title}");
            
            var task = new ProjectTask
            {
                Title = model.Title,
                Description = model.Description,
                TeamId = model.TeamId,
                AssignedBy = assignedByUserId,
                AssignedTo = model.AssignedTo,
                StartDate = model.StartDate,
                Deadline = model.Deadline,
                Status = "Pending",
                Priority = model.Priority
            };

            _context.ProjectTasks.Add(task);
            await _context.SaveChangesAsync();
            
            // Send notification if assigned to someone
            if (model.AssignedTo.HasValue)
            {
                await SendTaskAssignmentNotificationAsync(task.Id, assignedByUserId);
            }
            
            _logger.LogInformation($"Task created successfully with ID: {task.Id}");
            return task;
        }

        public async Task<ProjectTask> UpdateTaskAsync(ProjectTaskEditViewModel model)
        {
            _logger.LogInformation($"Updating task with ID {model.Id}");
            
            var task = await _context.ProjectTasks.FindAsync(model.Id);
            if (task == null)
                throw new ArgumentException($"Task with ID {model.Id} not found");

            task.Title = model.Title;
            task.Description = model.Description;
            task.TeamId = model.TeamId;
            task.AssignedTo = model.AssignedTo;
            task.StartDate = model.StartDate;
            task.Deadline = model.Deadline;
            task.Status = model.Status;
            task.Priority = model.Priority;

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Task with ID {model.Id} updated successfully");
            return task;
        }

        public async Task<bool> DeleteTaskAsync(int taskId)
        {
            _logger.LogInformation($"Deleting task with ID {taskId}");
            
            var task = await _context.ProjectTasks.FindAsync(taskId);
            if (task == null)
                return false;

            _context.ProjectTasks.Remove(task);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Task with ID {taskId} deleted successfully");
            return true;
        }

        public async Task<bool> UpdateTaskStatusAsync(int taskId, string status)
        {
            _logger.LogInformation($"Updating status for task {taskId} to {status}");
            
            var task = await _context.ProjectTasks.FindAsync(taskId);
            if (task == null)
                return false;

            task.Status = status;
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Status updated for task {taskId}");
            return true;
        }

        public async Task<IEnumerable<TeamListViewModel>> GetEmployeeTeamsAsync(int employeeId)
        {
            _logger.LogInformation($"Fetching teams for employee {employeeId}");
            
            return await _context.TeamMembers
                .Include(tm => tm.Team)
                    .ThenInclude(t => t.Department)
                .Include(tm => tm.Team)
                    .ThenInclude(t => t.Creator)
                .Where(tm => tm.EmployeeId == employeeId && tm.IsActive && tm.Team.IsActive)
                .Select(tm => new TeamListViewModel
                {
                    Id = tm.Team.Id,
                    Name = tm.Team.Name,
                    Description = tm.Team.Description,
                    DepartmentName = tm.Team.Department != null ? tm.Team.Department.Name : "N/A",
                    CreatorName = tm.Team.Creator != null ? tm.Team.Creator.FullName : "Unknown",
                    CreatedAt = tm.Team.CreatedAt,
                    MemberCount = tm.Team.TeamMembers.Count(m => m.IsActive),
                    TaskCount = tm.Team.Tasks.Count,
                    IsActive = tm.Team.IsActive
                })
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<ProjectTaskViewModel>> GetEmployeeTasksAsync(int employeeId)
        {
            _logger.LogInformation($"Fetching tasks for employee {employeeId}");
            
            // Tasks assigned directly to employee
            var directTasks = await _context.ProjectTasks
                .Include(t => t.Team)
                .Include(t => t.AssignedEmployee)
                .Where(t => t.AssignedTo == employeeId)
                .Select(t => new ProjectTaskViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    TeamId = t.TeamId,
                    TeamName = t.Team != null ? t.Team.Name : "N/A",
                    AssignedTo = t.AssignedTo,
                    AssignedEmployeeName = t.AssignedEmployee != null ? t.AssignedEmployee.FullName : "Unknown",
                    StartDate = t.StartDate,
                    Deadline = t.Deadline,
                    Status = t.Status,
                    Priority = t.Priority,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            // Tasks from teams where employee is a member (but not directly assigned)
            var teamTasks = await _context.TeamMembers
                .Include(tm => tm.Team)
                    .ThenInclude(t => t.Tasks)
                        .ThenInclude(task => task.AssignedEmployee)
                .Where(tm => tm.EmployeeId == employeeId && tm.IsActive && tm.Team.IsActive)
                .SelectMany(tm => tm.Team.Tasks)
                .Where(t => t.AssignedTo != employeeId) // Exclude already included direct tasks
                .Select(t => new ProjectTaskViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    TeamId = t.TeamId,
                    TeamName = t.Team != null ? t.Team.Name : "N/A",
                    AssignedTo = t.AssignedTo,
                    AssignedEmployeeName = t.AssignedEmployee != null ? t.AssignedEmployee.FullName : "Unknown",
                    StartDate = t.StartDate,
                    Deadline = t.Deadline,
                    Status = t.Status,
                    Priority = t.Priority,
                    CreatedAt = t.CreatedAt
                })
                .Distinct()
                .ToListAsync();

            return directTasks.Concat(teamTasks).OrderByDescending(t => t.Deadline).ToList();
        }

        public async Task SendTeamCreationNotificationAsync(int teamId, int createdByUserId)
        {
            _logger.LogInformation($"Sending team creation notifications for team {teamId}");
            
            var team = await _context.Teams
                .Include(t => t.TeamMembers)
                    .ThenInclude(tm => tm.Employee)
                .Include(t => t.Creator)
                .FirstOrDefaultAsync(t => t.Id == teamId);

            if (team == null)
            {
                _logger.LogWarning($"Team {teamId} not found for notification");
                return;
            }

            var creator = await _context.Employees.FindAsync(createdByUserId);
            var creatorName = creator != null ? creator.FullName : "System";

            foreach (var teamMember in team.TeamMembers.Where(tm => tm.IsActive))
            {
                // Get the ApplicationUser for this employee
                var user = await _context.Users.FirstOrDefaultAsync(u => u.EmployeeId == teamMember.EmployeeId);
                if (user == null) continue;

                var notification = new Notification
                {
                    UserId = user.Id,
                    Title = "New Team Assignment",
                    Message = $"You have been added to team '{team.Name}' by {creatorName}. Department: {team.Department?.Name ?? "N/A"}",
                    Type = NotificationType.Info,
                    Priority = NotificationPriority.Normal,
                    Module = "Teams",
                    Action = "ViewTeam",
                    ReferenceId = teamId.ToString(),
                    LinkUrl = $"/Teams/Details/{teamId}",
                    CreatedBy = createdByUserId.ToString(),
                    CreatedAt = DateTime.Now
                };

                _context.Notifications.Add(notification);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Team creation notifications sent for team {teamId}");
        }

        public async Task SendTaskAssignmentNotificationAsync(int taskId, int assignedByUserId)
        {
            _logger.LogInformation($"Sending task assignment notification for task {taskId}");
            
            var task = await _context.ProjectTasks
                .Include(t => t.AssignedEmployee)
                .Include(t => t.Team)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null || task.AssignedEmployee == null)
            {
                _logger.LogWarning($"Task {taskId} or assigned employee not found for notification");
                return;
            }

            var assigner = await _context.Employees.FindAsync(assignedByUserId);
            var assignerName = assigner != null ? assigner.FullName : "System";

            // Get the ApplicationUser for the assigned employee
            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmployeeId == task.AssignedTo);
            if (user == null)
            {
                _logger.LogWarning($"User not found for employee {task.AssignedTo}");
                return;
            }

            // Convert priority int to string for comparison
            string priorityLevel = task.Priority switch
            {
                1 => "Low",
                2 => "Medium",
                3 => "High",
                4 => "Critical",
                _ => "Normal"
            };

            var notification = new Notification
            {
                UserId = user.Id,
                Title = "New Task Assignment",
                Message = $"You have been assigned task '{task.Title}' by {assignerName}. Team: {task.Team?.Name ?? "N/A"}, Due: {task.Deadline:yyyy-MM-dd}",
                Type = NotificationType.Info,
                Priority = priorityLevel == "High" || priorityLevel == "Critical" ? NotificationPriority.High : NotificationPriority.Normal,
                Module = "Tasks",
                Action = "ViewTask",
                ReferenceId = taskId.ToString(),
                LinkUrl = $"/Teams/MyTasks",
                CreatedBy = assignedByUserId.ToString(),
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Task assignment notification sent for task {taskId}");
        }

        public async Task<Dictionary<int, string>> GetAvailableDepartmentsAsync()
        {
            _logger.LogInformation("Fetching available departments");
            
            return await _context.Departments
                .Where(d => d.IsActive)
                .OrderBy(d => d.Name)
                .ToDictionaryAsync(d => d.Id, d => d.Name);
        }

        public async Task<Dictionary<int, string>> GetAvailableEmployeesAsync()
        {
            _logger.LogInformation("Fetching available employees");
            
            return await _context.Employees
                .Where(e => e.IsActive)
                .OrderBy(e => e.FullName)
                .ToDictionaryAsync(e => e.Id, e => $"{e.FullName} ({e.Emp_ID})");
        }

        public async Task<int> GetTeamMemberCountAsync(int teamId)
        {
            return await _context.TeamMembers
                .CountAsync(tm => tm.TeamId == teamId && tm.IsActive);
        }

        public async Task<int> GetTeamTaskCountAsync(int teamId)
        {
            return await _context.ProjectTasks
                .CountAsync(t => t.TeamId == teamId);
        }

        // Task extension request operations
        public async Task<TaskExtensionRequest> CreateExtensionRequestAsync(int taskId, int requestedByEmployeeId, DateTime requestedDeadline, string reason)
        {
            _logger.LogInformation($"Creating extension request for task {taskId} by employee {requestedByEmployeeId}");
            
            var task = await _context.ProjectTasks.FindAsync(taskId);
            if (task == null)
                throw new ArgumentException($"Task with ID {taskId} not found");

            if (requestedDeadline <= task.Deadline)
                throw new ArgumentException("Requested deadline must be after the original deadline");

            var request = new TaskExtensionRequest
            {
                TaskId = taskId,
                RequestedByEmployeeId = requestedByEmployeeId,
                RequestedDeadline = requestedDeadline,
                Reason = reason,
                Status = ExtensionRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.TaskExtensionRequests.Add(request);
            task.ExtensionRequestCount++;
            task.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Extension request {request.Id} created successfully");
            return request;
        }

        public async Task<IEnumerable<TaskExtensionRequest>> GetExtensionRequestsByTaskAsync(int taskId)
        {
            return await _context.TaskExtensionRequests
                .Include(r => r.RequestedByEmployee)
                .Include(r => r.ReviewedByEmployee)
                .Where(r => r.TaskId == taskId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskExtensionRequest>> GetExtensionRequestsByEmployeeAsync(int employeeId)
        {
            return await _context.TaskExtensionRequests
                .Include(r => r.Task)
                .Include(r => r.ReviewedByEmployee)
                .Where(r => r.RequestedByEmployeeId == employeeId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<TaskExtensionRequest?> GetExtensionRequestByIdAsync(int requestId)
        {
            return await _context.TaskExtensionRequests
                .Include(r => r.Task)
                .Include(r => r.RequestedByEmployee)
                .Include(r => r.ReviewedByEmployee)
                .FirstOrDefaultAsync(r => r.Id == requestId);
        }

        public async Task<TaskExtensionRequest> ApproveExtensionRequestAsync(int requestId, int reviewedByEmployeeId, string? comments = null)
        {
            _logger.LogInformation($"Approving extension request {requestId}");
            
            var request = await _context.TaskExtensionRequests
                .Include(r => r.Task)
                .FirstOrDefaultAsync(r => r.Id == requestId);
            
            if (request == null)
                throw new ArgumentException($"Extension request with ID {requestId} not found");

            if (request.Status != ExtensionRequestStatus.Pending)
                throw new InvalidOperationException($"Extension request is already {request.Status}");

            var task = request.Task;
            if (task == null)
                throw new InvalidOperationException($"Task not found for extension request {requestId}");

            request.Status = ExtensionRequestStatus.Approved;
            request.ReviewedByEmployeeId = reviewedByEmployeeId;
            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewComments = comments;
            
            // Update task's extended deadline
            task.ExtendedDeadline = request.RequestedDeadline;
            task.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            // Apply performance deduction
            await ApplyPerformanceDeductionAsync(requestId);
            
            _logger.LogInformation($"Extension request {requestId} approved");
            return request;
        }

        public async Task<TaskExtensionRequest> RejectExtensionRequestAsync(int requestId, int reviewedByEmployeeId, string? comments = null)
        {
            _logger.LogInformation($"Rejecting extension request {requestId}");
            
            var request = await _context.TaskExtensionRequests.FindAsync(requestId);
            if (request == null)
                throw new ArgumentException($"Extension request with ID {requestId} not found");

            if (request.Status != ExtensionRequestStatus.Pending)
                throw new InvalidOperationException($"Extension request is already {request.Status}");

            request.Status = ExtensionRequestStatus.Rejected;
            request.ReviewedByEmployeeId = reviewedByEmployeeId;
            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewComments = comments;
            
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Extension request {requestId} rejected");
            return request;
        }

        public async Task<bool> ApplyPerformanceDeductionAsync(int requestId)
        {
            _logger.LogInformation($"Applying performance deduction for extension request {requestId}");
            
            var request = await _context.TaskExtensionRequests
                .Include(r => r.Task)
                .ThenInclude(t => t.AssignedEmployee)
                .FirstOrDefaultAsync(r => r.Id == requestId);
            
            if (request == null)
                throw new ArgumentException($"Extension request with ID {requestId} not found");

            if (request.Status != ExtensionRequestStatus.Approved)
                throw new InvalidOperationException("Performance deduction can only be applied to approved extension requests");

            if (request.IsDeductionApplied)
                return true; // Already applied

            // Calculate deduction based on extension days
            var task = request.Task;
            if (task == null || task.AssignedEmployee == null)
                return false;

            var originalDeadline = task.Deadline;
            var extendedDeadline = request.RequestedDeadline;
            var extensionDays = (extendedDeadline - originalDeadline).TotalDays;

            // Deduction formula: 0.5 points per day of extension, max 10 points
            var deduction = Math.Min((decimal)extensionDays * 0.5m, 10.0m);
            request.PerformanceDeduction = deduction;
            request.IsDeductionApplied = true;
            request.DeductionAppliedAt = DateTime.UtcNow;
            
            // TODO: Apply deduction to employee's performance record
            // This would require integration with performance service
            // For now, we'll just mark the task with penalty flag
            task.PerformancePenaltyApplied = true;
            
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Performance deduction of {deduction} applied for extension request {requestId}");
            return true;
        }
    }
}
