using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using HR_Management_System.Models;
using HR_Management_System.Models.ViewModels;
using HR_Management_System.Services.Interfaces;
using System.Security.Claims;

namespace HR_Management_System.Controllers
{
    [Authorize]
    public class TeamController : Controller
    {
        private readonly ITeamService _teamService;
        private readonly ILogger<TeamController> _logger;

        public TeamController(ITeamService teamService, ILogger<TeamController> logger)
        {
            _teamService = teamService;
            _logger = logger;
        }

        // GET: Team
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var teams = await _teamService.GetAllTeamsAsync();
                return View(teams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching teams");
                TempData["ErrorMessage"] = "An error occurred while fetching teams.";
                return View(new List<TeamListViewModel>());
            }
        }

        // GET: Team/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var team = await _teamService.GetTeamByIdAsync(id);
                if (team == null)
                {
                    TempData["ErrorMessage"] = "Team not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if current user is a member of this team or has admin/HR role
                var currentUserId = GetCurrentEmployeeId();
                var isTeamMember = team.Members.Any(m => m.EmployeeId == currentUserId);
                var isAdminOrHR = User.IsInRole("Admin") || User.IsInRole("HRManager");

                if (!isTeamMember && !isAdminOrHR)
                {
                    TempData["ErrorMessage"] = "You don't have permission to view this team.";
                    return RedirectToAction(nameof(MyTeams));
                }

                return View(team);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching team details for ID {id}");
                TempData["ErrorMessage"] = "An error occurred while fetching team details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Team/Create
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var model = new TeamCreateViewModel
                {
                    AvailableDepartments = await _teamService.GetAvailableDepartmentsAsync(),
                    AvailableEmployees = await _teamService.GetAvailableEmployeesAsync()
                };
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create team form");
                TempData["ErrorMessage"] = "An error occurred while loading the form.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Team/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> Create(TeamCreateViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var currentUserId = GetCurrentEmployeeId();
                    var team = await _teamService.CreateTeamAsync(model, currentUserId);

                    // Send notifications to team members
                    await _teamService.SendTeamCreationNotificationAsync(team.Id, currentUserId);

                    TempData["SuccessMessage"] = $"Team '{team.Name}' created successfully!";
                    return RedirectToAction(nameof(Details), new { id = team.Id });
                }

                // Reload dropdown data if validation fails
                model.AvailableDepartments = await _teamService.GetAvailableDepartmentsAsync();
                model.AvailableEmployees = await _teamService.GetAvailableEmployeesAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating team");
                TempData["ErrorMessage"] = $"An error occurred while creating the team: {ex.Message}";
                
                model.AvailableDepartments = await _teamService.GetAvailableDepartmentsAsync();
                model.AvailableEmployees = await _teamService.GetAvailableEmployeesAsync();
                return View(model);
            }
        }

        // GET: Team/Edit/5
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var team = await _teamService.GetTeamByIdAsync(id);
                if (team == null)
                {
                    TempData["ErrorMessage"] = "Team not found.";
                    return RedirectToAction(nameof(Index));
                }

                var model = new TeamEditViewModel
                {
                    Id = team.Id,
                    Name = team.Name,
                    Description = team.Description,
                    DepartmentId = team.DepartmentId,
                    MemberIds = team.Members.Select(m => m.EmployeeId).ToList(),
                    CreatedAt = team.CreatedAt,
                    CreatorName = team.CreatorName,
                    DepartmentName = team.DepartmentName,
                    AvailableDepartments = await _teamService.GetAvailableDepartmentsAsync(),
                    AvailableEmployees = await _teamService.GetAvailableEmployeesAsync()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading edit form for team ID {id}");
                TempData["ErrorMessage"] = "An error occurred while loading the edit form.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Team/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> Edit(int id, TeamEditViewModel model)
        {
            if (id != model.Id)
            {
                TempData["ErrorMessage"] = "Team ID mismatch.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var team = await _teamService.UpdateTeamAsync(model);
                    TempData["SuccessMessage"] = $"Team '{team.Name}' updated successfully!";
                    return RedirectToAction(nameof(Details), new { id = team.Id });
                }

                // Reload dropdown data if validation fails
                model.AvailableDepartments = await _teamService.GetAvailableDepartmentsAsync();
                model.AvailableEmployees = await _teamService.GetAvailableEmployeesAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating team ID {id}");
                TempData["ErrorMessage"] = $"An error occurred while updating the team: {ex.Message}";
                
                model.AvailableDepartments = await _teamService.GetAvailableDepartmentsAsync();
                model.AvailableEmployees = await _teamService.GetAvailableEmployeesAsync();
                return View(model);
            }
        }

        // POST: Team/Deactivate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> Deactivate(int id)
        {
            try
            {
                var success = await _teamService.DeactivateTeamAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "Team deactivated successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to deactivate team.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deactivating team ID {id}");
                TempData["ErrorMessage"] = "An error occurred while deactivating the team.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Team/Activate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> Activate(int id)
        {
            try
            {
                var success = await _teamService.ActivateTeamAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "Team activated successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to activate team.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error activating team ID {id}");
                TempData["ErrorMessage"] = "An error occurred while activating the team.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Team/MyTeams
        [Authorize]
        public async Task<IActionResult> MyTeams()
        {
            try
            {
                var currentEmployeeId = GetCurrentEmployeeId();
                var teams = await _teamService.GetEmployeeTeamsAsync(currentEmployeeId);
                return View(teams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching employee teams");
                TempData["ErrorMessage"] = "An error occurred while fetching your teams.";
                return View(new List<TeamListViewModel>());
            }
        }

        // GET: Team/MyTasks
        [Authorize]
        public async Task<IActionResult> MyTasks()
        {
            try
            {
                var currentEmployeeId = GetCurrentEmployeeId();
                var tasks = await _teamService.GetEmployeeTasksAsync(currentEmployeeId);
                return View(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching employee tasks");
                TempData["ErrorMessage"] = "An error occurred while fetching your tasks.";
                return View(new List<ProjectTaskViewModel>());
            }
        }

        // GET: Team/CreateTask
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> CreateTask(int teamId)
        {
            try
            {
                var team = await _teamService.GetTeamByIdAsync(teamId);
                if (team == null)
                {
                    TempData["ErrorMessage"] = "Team not found.";
                    return RedirectToAction(nameof(Index));
                }

                var model = new ProjectTaskCreateViewModel
                {
                    TeamId = teamId,
                    AvailableTeamMembers = team.Members
                        .ToDictionary(m => m.EmployeeId, m => $"{m.EmployeeName} ({m.EmployeeCode})")
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading create task form for team ID {teamId}");
                TempData["ErrorMessage"] = "An error occurred while loading the task form.";
                return RedirectToAction(nameof(Details), new { id = teamId });
            }
        }

        // POST: Team/CreateTask
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> CreateTask(ProjectTaskCreateViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var currentUserId = GetCurrentEmployeeId();
                    var task = await _teamService.CreateTaskAsync(model, currentUserId);

                    // Send notification if task is assigned to someone
                    if (task.AssignedTo.HasValue)
                    {
                        await _teamService.SendTaskAssignmentNotificationAsync(task.Id, currentUserId);
                    }

                    TempData["SuccessMessage"] = $"Task '{task.Title}' created successfully!";
                    return RedirectToAction(nameof(Details), new { id = model.TeamId });
                }

                // Reload dropdown data if validation fails
                var team = await _teamService.GetTeamByIdAsync(model.TeamId);
                if (team != null)
                {
                    model.AvailableTeamMembers = team.Members
                        .ToDictionary(m => m.EmployeeId, m => $"{m.EmployeeName} ({m.EmployeeCode})");
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                TempData["ErrorMessage"] = $"An error occurred while creating the task: {ex.Message}";
                
                // Reload dropdown data
                var team = await _teamService.GetTeamByIdAsync(model.TeamId);
                if (team != null)
                {
                    model.AvailableTeamMembers = team.Members
                        .ToDictionary(m => m.EmployeeId, m => $"{m.EmployeeName} ({m.EmployeeCode})");
                }

                return View(model);
            }
        }

        // GET: Team/EditTask/5
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> EditTask(int id)
        {
            try
            {
                var task = await _teamService.GetTaskByIdAsync(id);
                if (task == null)
                {
                    TempData["ErrorMessage"] = "Task not found.";
                    return RedirectToAction(nameof(Index));
                }

                var team = await _teamService.GetTeamByIdAsync(task.TeamId);
                var model = new ProjectTaskEditViewModel
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    TeamId = task.TeamId,
                    AssignedTo = task.AssignedTo,
                    StartDate = task.StartDate,
                    Deadline = task.Deadline,
                    Priority = task.Priority,
                    Status = task.Status,
                    TeamName = task.TeamName,
                    AssignedEmployeeName = task.AssignedEmployeeName,
                    CreatedAt = task.CreatedAt,
                    AvailableTeamMembers = team?.Members
                        .ToDictionary(m => m.EmployeeId, m => $"{m.EmployeeName} ({m.EmployeeCode})") ?? new Dictionary<int, string>()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading edit form for task ID {id}");
                TempData["ErrorMessage"] = "An error occurred while loading the task edit form.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Team/EditTask/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> EditTask(int id, ProjectTaskEditViewModel model)
        {
            if (id != model.Id)
            {
                TempData["ErrorMessage"] = "Task ID mismatch.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var task = await _teamService.UpdateTaskAsync(model);
                    TempData["SuccessMessage"] = $"Task '{task.Title}' updated successfully!";
                    return RedirectToAction(nameof(Details), new { id = model.TeamId });
                }

                // Reload dropdown data if validation fails
                var team = await _teamService.GetTeamByIdAsync(model.TeamId);
                if (team != null)
                {
                    model.AvailableTeamMembers = team.Members
                        .ToDictionary(m => m.EmployeeId, m => $"{m.EmployeeName} ({m.EmployeeCode})");
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating task ID {id}");
                TempData["ErrorMessage"] = $"An error occurred while updating the task: {ex.Message}";
                
                // Reload dropdown data
                var team = await _teamService.GetTeamByIdAsync(model.TeamId);
                if (team != null)
                {
                    model.AvailableTeamMembers = team.Members
                        .ToDictionary(m => m.EmployeeId, m => $"{m.EmployeeName} ({m.EmployeeCode})");
                }

                return View(model);
            }
        }

        // POST: Team/UpdateTaskStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> UpdateTaskStatus(int id, string status)
        {
            try
            {
                var task = await _teamService.GetTaskByIdAsync(id);
                if (task == null)
                {
                    TempData["ErrorMessage"] = "Task not found.";
                    return RedirectToAction(nameof(MyTasks));
                }

                // Check if current user is assigned to this task or is admin/HR
                var currentEmployeeId = GetCurrentEmployeeId();
                var isAssigned = task.AssignedTo == currentEmployeeId;
                var isAdminOrHR = User.IsInRole("Admin") || User.IsInRole("HRManager");

                if (!isAssigned && !isAdminOrHR)
                {
                    TempData["ErrorMessage"] = "You don't have permission to update this task.";
                    return RedirectToAction(nameof(MyTasks));
                }

                var success = await _teamService.UpdateTaskStatusAsync(id, status);
                if (success)
                {
                    TempData["SuccessMessage"] = $"Task status updated to '{status}'.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update task status.";
                }

                return RedirectToAction(nameof(MyTasks));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating task status for task ID {id}");
                TempData["ErrorMessage"] = "An error occurred while updating task status.";
                return RedirectToAction(nameof(MyTasks));
            }
        }

        // Task Extension Request Actions
        [HttpGet("Task/{taskId}/RequestExtension")]
        [Authorize]
        public async Task<IActionResult> RequestExtension(int taskId)
        {
            try
            {
                var task = await _teamService.GetTaskByIdAsync(taskId);
                if (task == null)
                {
                    TempData["ErrorMessage"] = "Task not found.";
                    return RedirectToAction(nameof(MyTasks));
                }

                // Check if user is assigned to this task
                var currentEmployeeId = GetCurrentEmployeeId();
                if (task.AssignedTo != currentEmployeeId && !User.IsInRole("Admin") && !User.IsInRole("HR"))
                {
                    TempData["ErrorMessage"] = "You are not authorized to request extension for this task.";
                    return RedirectToAction(nameof(MyTasks));
                }

                var model = new TaskExtensionRequestCreateViewModel
                {
                    TaskId = taskId,
                    RequestedDeadline = task.Deadline.AddDays(7) // Default: 7 days extension
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading extension request form for task {taskId}");
                TempData["ErrorMessage"] = "An error occurred while loading the extension request form.";
                return RedirectToAction(nameof(MyTasks));
            }
        }

        [HttpPost("Task/{taskId}/RequestExtension")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> RequestExtension(int taskId, TaskExtensionRequestCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var currentEmployeeId = GetCurrentEmployeeId();
                var request = await _teamService.CreateExtensionRequestAsync(taskId, currentEmployeeId, model.RequestedDeadline, model.Reason);
                
                TempData["SuccessMessage"] = "Extension request submitted successfully. It will be reviewed by your manager.";
                return RedirectToAction(nameof(MyTasks));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating extension request for task {taskId}");
                TempData["ErrorMessage"] = $"An error occurred while submitting extension request: {ex.Message}";
                return View(model);
            }
        }

        [HttpGet("Task/ExtensionRequests")]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> ExtensionRequests()
        {
            try
            {
                var requests = await _teamService.GetExtensionRequestsByEmployeeAsync(GetCurrentEmployeeId());
                var viewModels = requests.Select(r => new TaskExtensionRequestViewModel
                {
                    Id = r.Id,
                    TaskId = r.TaskId,
                    TaskTitle = r.Task?.Title ?? "Unknown Task",
                    RequestedByEmployeeId = r.RequestedByEmployeeId,
                    RequestedByEmployeeName = r.RequestedByEmployee?.FullName ?? "Unknown",
                    RequestedDeadline = r.RequestedDeadline,
                    Reason = r.Reason,
                    Status = (int)r.Status,
                    StatusName = r.Status.ToString(),
                    ReviewedByEmployeeId = r.ReviewedByEmployeeId,
                    ReviewedByEmployeeName = r.ReviewedByEmployee?.FullName,
                    ReviewedAt = r.ReviewedAt,
                    ReviewComments = r.ReviewComments,
                    PerformanceDeduction = r.PerformanceDeduction,
                    IsDeductionApplied = r.IsDeductionApplied,
                    CreatedAt = r.CreatedAt
                }).ToList();

                return View(viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching extension requests");
                TempData["ErrorMessage"] = "An error occurred while fetching extension requests.";
                return View(new List<TaskExtensionRequestViewModel>());
            }
        }

        [HttpGet("Task/ExtensionRequest/{id}/Review")]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> ReviewExtensionRequest(int id)
        {
            try
            {
                var request = await _teamService.GetExtensionRequestByIdAsync(id);
                if (request == null)
                {
                    TempData["ErrorMessage"] = "Extension request not found.";
                    return RedirectToAction(nameof(ExtensionRequests));
                }

                var model = new TaskExtensionRequestReviewViewModel
                {
                    RequestId = request.Id
                };

                ViewBag.Request = request;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading extension request review for ID {id}");
                TempData["ErrorMessage"] = "An error occurred while loading the extension request.";
                return RedirectToAction(nameof(ExtensionRequests));
            }
        }

        [HttpPost("Task/ExtensionRequest/{id}/Review")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOrHR")]
        public async Task<IActionResult> ReviewExtensionRequest(int id, TaskExtensionRequestReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var currentEmployeeId = GetCurrentEmployeeId();
                TaskExtensionRequest request;
                
                if (model.Approve)
                {
                    request = await _teamService.ApproveExtensionRequestAsync(id, currentEmployeeId, model.Comments);
                    TempData["SuccessMessage"] = "Extension request approved successfully.";
                }
                else
                {
                    request = await _teamService.RejectExtensionRequestAsync(id, currentEmployeeId, model.Comments);
                    TempData["SuccessMessage"] = "Extension request rejected successfully.";
                }

                return RedirectToAction(nameof(ExtensionRequests));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error reviewing extension request {id}");
                TempData["ErrorMessage"] = $"An error occurred while reviewing extension request: {ex.Message}";
                return View(model);
            }
        }

        // API endpoint for dashboard
        [HttpGet("/api/teams/active-count")]
        [Authorize]
        public async Task<IActionResult> GetActiveTeamCount()
        {
            try
            {
                var teams = await _teamService.GetAllTeamsAsync();
                var activeTeams = teams?.Count(t => t.IsActive) ?? 0;
                return Ok(new { count = activeTeams });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching active team count");
                return StatusCode(500, new { error = "An error occurred while fetching team count" });
            }
        }

        // Helper method to get current employee ID
        private int GetCurrentEmployeeId()
        {
            var employeeIdClaim = User.FindFirstValue("EmployeeId");
            if (int.TryParse(employeeIdClaim, out int employeeId))
            {
                return employeeId;
            }

            // Fallback: try to get from UserId claim
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogWarning($"EmployeeId claim not found for user {userId}. Using default value 1.");
            return 1; // Default fallback - in production, this should be handled properly
        }
    }
}