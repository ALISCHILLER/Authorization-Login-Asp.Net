using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;

namespace Authorization_Login_Asp.Net.Core.Application.Filters
{
    /// <summary>
    /// فیلتر برای اندازه‌گیری زمان اجرای اکشن‌ها
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class PerformanceFilterAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string _actionName;
        private readonly long _thresholdMilliseconds;

        public PerformanceFilterAttribute(string actionName = null, long thresholdMilliseconds = 1000)
        {
            _actionName = actionName;
            _thresholdMilliseconds = thresholdMilliseconds;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var logger = context.HttpContext.RequestServices.GetService(typeof(ILogger<PerformanceFilterAttribute>)) as ILogger<PerformanceFilterAttribute>;
            var performanceService = context.HttpContext.RequestServices.GetService(typeof(IPerformanceService)) as IPerformanceService;

            if (performanceService == null)
            {
                logger?.LogError("PerformanceService not found in DI container");
                await next();
                return;
            }

            var actionName = _actionName ?? context.ActionDescriptor.DisplayName;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await next();
            }
            finally
            {
                stopwatch.Stop();
                var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

                // ثبت زمان اجرا
                await performanceService.LogPerformanceAsync(new PerformanceLogEntry
                {
                    ActionName = actionName,
                    ElapsedMilliseconds = elapsedMilliseconds,
                    Timestamp = DateTime.UtcNow,
                    RequestId = context.HttpContext.TraceIdentifier,
                    UserId = context.HttpContext.User.FindFirst("sub")?.Value,
                    Path = context.HttpContext.Request.Path.ToString(),
                    Method = context.HttpContext.Request.Method
                });

                // ثبت هشدار در صورت تجاوز از آستانه
                if (elapsedMilliseconds > _thresholdMilliseconds)
                {
                    logger?.LogWarning(
                        "Performance threshold exceeded for action {ActionName}. Elapsed time: {ElapsedMilliseconds}ms, Threshold: {ThresholdMilliseconds}ms",
                        actionName,
                        elapsedMilliseconds,
                        _thresholdMilliseconds
                    );
                }
            }
        }
    }

    public class PerformanceLogEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ActionName { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public DateTime Timestamp { get; set; }
        public string RequestId { get; set; }
        public string UserId { get; set; }
        public string Path { get; set; }
        public string Method { get; set; }
    }
} 