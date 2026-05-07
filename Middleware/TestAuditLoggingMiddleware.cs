using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HR_Management_System.Middleware
{
    public class TestAuditLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public TestAuditLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ILogger<TestAuditLoggingMiddleware> logger)
        {
            try
            {
                // Log using ILogger (will appear in Serilog)
                logger.LogInformation("TestAuditLoggingMiddleware START: {Method} {Path} from {RemoteIp}",
                    context.Request.Method, context.Request.Path, context.Connection.RemoteIpAddress);
                
                // Also write to console for debugging
                Console.WriteLine($"TestAuditLoggingMiddleware: {context.Request.Method} {context.Request.Path}");
                
                await _next(context);
                
                logger.LogInformation("TestAuditLoggingMiddleware END: {Method} {Path} -> {StatusCode}",
                    context.Request.Method, context.Request.Path, context.Response.StatusCode);
                
                Console.WriteLine($"TestAuditLoggingMiddleware: {context.Request.Method} {context.Request.Path} -> {context.Response.StatusCode}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "TestAuditLoggingMiddleware ERROR: {Method} {Path}",
                    context.Request.Method, context.Request.Path);
                Console.WriteLine($"TestAuditLoggingMiddleware ERROR: {ex.Message}");
                throw;
            }
        }
    }
}