using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;

namespace Authorization_Login_Asp.Net.Infrastructure.Middleware
{
    public class AuditLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditLoggingMiddleware> _logger;
        private readonly MiddlewareConfiguration _config;

        public AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger, MiddlewareConfiguration config)
        {
            _next = next;
            _logger = logger;
            _config = config;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!_config.EnableAuditLogging || !ShouldAudit(context))
            {
                await _next(context);
                return;
            }

            var auditInfo = new
            {
                Timestamp = DateTime.UtcNow,
                UserId = context.User?.FindFirst("sub")?.Value,
                UserName = context.User?.Identity?.Name,
                IP = context.Connection.RemoteIpAddress?.ToString(),
                Method = context.Request.Method,
                Path = context.Request.Path,
                QueryString = context.Request.QueryString.ToString(),
                StatusCode = context.Response.StatusCode,
                UserAgent = context.Request.Headers["User-Agent"].ToString(),
                CorrelationId = context.Request.Headers["X-Correlation-ID"].ToString()
            };

            try
            {
                await _next(context);

                // لاگ کردن عملیات موفق
                if (context.Response.StatusCode < 400)
                {
                    _logger.LogInformation("عملیات موفق: {AuditInfo}", JsonSerializer.Serialize(auditInfo));
                }
                else
                {
                    _logger.LogWarning("عملیات ناموفق: {AuditInfo}", JsonSerializer.Serialize(auditInfo));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در عملیات: {AuditInfo}", JsonSerializer.Serialize(auditInfo));
                throw;
            }
        }

        private bool ShouldAudit(HttpContext context)
        {
            // عملیات‌های مهم که باید لاگ شوند
            var importantOperations = new[]
            {
                "/api/auth/login",
                "/api/auth/register",
                "/api/auth/logout",
                "/api/auth/refresh-token",
                "/api/admin/users",
                "/api/admin/roles",
                "/api/admin/permissions"
            };

            return importantOperations.Any(op => context.Request.Path.StartsWithSegments(op)) ||
                   context.Request.Method == "POST" ||
                   context.Request.Method == "PUT" ||
                   context.Request.Method == "DELETE";
        }
    }
} 