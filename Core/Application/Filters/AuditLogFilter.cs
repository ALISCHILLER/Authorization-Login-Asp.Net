using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using System.Text.Json;

namespace Authorization_Login_Asp.Net.Core.Application.Filters
{
    /// <summary>
    /// فیلتر برای ثبت لاگ‌های حسابرسی
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuditLogAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string _actionName;
        private readonly bool _logRequest;
        private readonly bool _logResponse;

        public AuditLogAttribute(string actionName = null, bool logRequest = true, bool logResponse = true)
        {
            _actionName = actionName;
            _logRequest = logRequest;
            _logResponse = logResponse;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var logger = context.HttpContext.RequestServices.GetService(typeof(ILogger<AuditLogAttribute>)) as ILogger<AuditLogAttribute>;
            var auditService = context.HttpContext.RequestServices.GetService(typeof(IAuditService)) as IAuditService;

            if (auditService == null)
            {
                logger?.LogError("AuditService not found in DI container");
                await next();
                return;
            }

            var userId = context.HttpContext.User.FindFirst("sub")?.Value;
            var actionName = _actionName ?? context.ActionDescriptor.DisplayName;
            var timestamp = DateTime.UtcNow;
            var requestId = context.HttpContext.TraceIdentifier;

            // ثبت اطلاعات درخواست
            if (_logRequest)
            {
                var requestData = new
                {
                    Method = context.HttpContext.Request.Method,
                    Path = context.HttpContext.Request.Path,
                    QueryString = context.HttpContext.Request.QueryString.ToString(),
                    Headers = context.HttpContext.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                    Body = context.ActionArguments
                };

                await auditService.LogAuditAsync(new AuditLogEntry
                {
                    UserId = userId != null ? Guid.Parse(userId) : null,
                    Action = $"{actionName}_Request",
                    Timestamp = timestamp,
                    RequestId = requestId,
                    Data = JsonSerializer.Serialize(requestData),
                    IpAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = context.HttpContext.Request.Headers["User-Agent"].ToString()
                });
            }

            // اجرای اکشن
            var resultContext = await next();

            // ثبت اطلاعات پاسخ
            if (_logResponse && resultContext.Result is ObjectResult objectResult)
            {
                var responseData = new
                {
                    StatusCode = objectResult.StatusCode,
                    Value = objectResult.Value
                };

                await auditService.LogAuditAsync(new AuditLogEntry
                {
                    UserId = userId != null ? Guid.Parse(userId) : null,
                    Action = $"{actionName}_Response",
                    Timestamp = DateTime.UtcNow,
                    RequestId = requestId,
                    Data = JsonSerializer.Serialize(responseData),
                    IpAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = context.HttpContext.Request.Headers["User-Agent"].ToString()
                });
            }
        }
    }

    public class AuditLogEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? UserId { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }
        public string RequestId { get; set; }
        public string Data { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
    }
} 