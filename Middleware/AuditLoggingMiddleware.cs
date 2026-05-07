using HR_Management_System.Data;
using HR_Management_System.Models;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HR_Management_System.Middleware
{
    public class AuditLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditLoggingMiddleware> _logger;

        public AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            // Skip logging for static files, health checks, and certain paths
            if (ShouldSkipLogging(context))
            {
                await _next(context);
                return;
            }

            _logger.LogInformation("AuditLoggingMiddleware: Processing request {Method} {Path}", context.Request.Method, context.Request.Path);

            var stopwatch = Stopwatch.StartNew();
            var request = context.Request;
            var response = context.Response;

            // Get user information
            var userName = context.User?.Identity?.IsAuthenticated == true
                ? context.User.Identity.Name
                : "Anonymous";
            
            var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRoles = context.User?.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            // Extract controller and action from route data
            string? controller = null;
            string? action = null;
            
            var routeData = context.GetRouteData();
            if (routeData != null)
            {
                controller = routeData.Values["controller"]?.ToString();
                action = routeData.Values["action"]?.ToString();
            }

            // Create audit log entry
            var auditLog = new AuditLog
            {
                TimeStamp = DateTime.UtcNow,
                UserName = userName,
                Controller = controller,
                Action = action,
                HttpMethod = request.Method,
                RequestPath = request.Path + request.QueryString,
                Level = "Information",
                Message = $"{request.Method} {request.Path}",
                RequestId = Activity.Current?.Id ?? context.TraceIdentifier,
                TraceId = context.TraceIdentifier,
                Properties = $"{{" +
                    $"\"UserId\": \"{userId}\"," +
                    $"\"UserRoles\": \"{string.Join(",", userRoles ?? new List<string>())}\"," +
                    $"\"UserAgent\": \"{request.Headers.UserAgent}\"," +
                    $"\"Host\": \"{request.Host}\"," +
                    $"\"Scheme\": \"{request.Scheme}\"," +
                    $"\"ContentType\": \"{request.ContentType}\"" +
                    $"}}"
            };

            try
            {
                // Process the request
                await _next(context);
                
                stopwatch.Stop();
                
                // Update audit log with response info
                auditLog.Message = $"{request.Method} {request.Path} - {response.StatusCode} ({stopwatch.ElapsedMilliseconds}ms)";
                
                // Log exceptions if any
                var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
                if (exception != null)
                {
                    auditLog.Level = "Error";
                    auditLog.Exception = exception.ToString();
                    auditLog.Message = $"{request.Method} {request.Path} - Exception: {exception.Message}";
                }
                else if (response.StatusCode >= 400)
                {
                    auditLog.Level = response.StatusCode >= 500 ? "Error" : "Warning";
                }
                
                // Save to database
                _logger.LogInformation("AuditLoggingMiddleware: Saving audit log for {Method} {Path}", request.Method, request.Path);
                await dbContext.AuditLogs.AddAsync(auditLog);
                await dbContext.SaveChangesAsync();
                _logger.LogInformation("AuditLoggingMiddleware: Audit log saved successfully");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                // Log the exception
                auditLog.Level = "Error";
                auditLog.Exception = ex.ToString();
                auditLog.Message = $"{request.Method} {request.Path} - Unhandled Exception: {ex.Message}";
                
                try
                {
                    _logger.LogError(ex, "AuditLoggingMiddleware: Error processing request, attempting to save error log");
                    await dbContext.AuditLogs.AddAsync(auditLog);
                    await dbContext.SaveChangesAsync();
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "AuditLoggingMiddleware: Failed to save audit log to database");
                }
                
                throw; // Re-throw the original exception
            }
        }

        private bool ShouldSkipLogging(HttpContext context)
        {
            var path = context.Request.Path.Value ?? "";
            
            // Skip static files
            if (path.StartsWith("/lib/") || 
                path.StartsWith("/css/") || 
                path.StartsWith("/js/") || 
                path.StartsWith("/images/") ||
                path.StartsWith("/fonts/") ||
                path.EndsWith(".css") ||
                path.EndsWith(".js") ||
                path.EndsWith(".png") ||
                path.EndsWith(".jpg") ||
                path.EndsWith(".jpeg") ||
                path.EndsWith(".gif") ||
                path.EndsWith(".ico") ||
                path.EndsWith(".svg"))
            {
                return true;
            }
            
            // Skip health checks and favicon
            if (path.Contains("health") || path.Contains("favicon.ico"))
            {
                return true;
            }
            
            return false;
        }
    }
}