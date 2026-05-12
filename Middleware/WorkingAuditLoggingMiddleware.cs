using HR_Management_System.Data;
using HR_Management_System.Models;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace HR_Management_System.Middleware
{
    public class WorkingAuditLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public WorkingAuditLoggingMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory)
        {
            _next = next;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task InvokeAsync(HttpContext context, ILogger<WorkingAuditLoggingMiddleware> logger)
        {
            // Skip logging for static files and certain paths
            if (ShouldSkipLogging(context))
            {
                await _next(context);
                return;
            }

            logger.LogInformation("WorkingAuditLoggingMiddleware: Processing {Method} {Path}", context.Request.Method, context.Request.Path);
            
            var stopwatch = Stopwatch.StartNew();
            var request = context.Request;
            var response = context.Response;

            // Get user information
            string userName = "Anonymous";
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                userName = context.User.Identity.Name;
                // Try to get email claim if name is empty
                if (string.IsNullOrEmpty(userName))
                {
                    var emailClaim = context.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email);
                    if (emailClaim != null)
                        userName = emailClaim.Value;
                    else
                    {
                        var nameIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                        if (nameIdClaim != null)
                            userName = nameIdClaim.Value;
                    }
                }
            }

            try
            {
                // Process the request first
                await _next(context);
                
                stopwatch.Stop();
                
                // Now we can try to get route data (after routing has happened)
                string? controller = null;
                string? action = null;
                
                var routeData = context.GetRouteData();
                if (routeData != null)
                {
                    controller = routeData.Values["controller"]?.ToString();
                    action = routeData.Values["action"]?.ToString();
                }

                // Capture all values as strings before background task
                var httpMethod = request.Method;
                var requestPath = request.Path + request.QueryString;
                var statusCode = response.StatusCode;
                var elapsedMs = stopwatch.ElapsedMilliseconds;
                var level = statusCode >= 400 ? (statusCode >= 500 ? "Error" : "Warning") : "Information";
                var message = $"{httpMethod} {request.Path} - {statusCode} ({elapsedMs}ms)";
                var requestId = Truncate(Activity.Current?.Id ?? context.TraceIdentifier, 50);
                var traceId = Truncate(context.TraceIdentifier, 50);

                // Create audit log entry with captured values
                var auditLog = new AuditLog
                {
                    TimeStamp = DateTime.UtcNow,
                    UserName = userName,
                    Controller = controller,
                    Action = action,
                    HttpMethod = httpMethod,
                    RequestPath = requestPath,
                    Level = level,
                    Message = message,
                    RequestId = requestId,
                    TraceId = traceId
                };

                // Save to database in background using a new scope
                _ = Task.Run(async () =>
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        try
                        {
                            await dbContext.AuditLogs.AddAsync(auditLog);
                            await dbContext.SaveChangesAsync();
                            logger.LogInformation("WorkingAuditLoggingMiddleware: Audit log saved for {Method} {Path}", httpMethod, requestPath);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "WorkingAuditLoggingMiddleware: Failed to save audit log to database");
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                // Capture values before using them in logging
                var method = request.Method;
                var path = request.Path;
                var queryString = request.QueryString;
                var requestPath = path + queryString;
                var message = $"{method} {path} - Unhandled Exception: {ex.Message}";
                var exceptionStr = ex.ToString();
                var requestId = Truncate(Activity.Current?.Id ?? context.TraceIdentifier, 50);
                var traceId = Truncate(context.TraceIdentifier, 50);
                
                logger.LogError(ex, "WorkingAuditLoggingMiddleware: Error processing request {Method} {Path}", method, path);
                
                // Try to save error log using a new scope
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    try
                    {
                        var errorLog = new AuditLog
                        {
                            TimeStamp = DateTime.UtcNow,
                            UserName = userName,
                            HttpMethod = method,
                            RequestPath = requestPath,
                            Level = "Error",
                            Message = message,
                            Exception = exceptionStr,
                            RequestId = requestId,
                            TraceId = traceId
                        };
                        
                        await dbContext.AuditLogs.AddAsync(errorLog);
                        await dbContext.SaveChangesAsync();
                    }
                    catch (Exception dbEx)
                    {
                        logger.LogError(dbEx, "WorkingAuditLoggingMiddleware: Failed to save error audit log");
                    }
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
            
            // Skip the audit log API endpoint itself to avoid infinite recursion
            if (path.StartsWith("/api/auditlog") || path.StartsWith("/Logs"))
            {
                return true;
            }
            
            return false;
        }

        private static string? Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
                return value;
            
            return value.Substring(0, maxLength);
        }
    }
}