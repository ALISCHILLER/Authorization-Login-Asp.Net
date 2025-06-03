using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Authorization_Login_Asp.Net.Presentation.Api.Controllers
{
    /// <summary>
    /// کنترلر بررسی وضعیت سلامت سیستم
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class HealthController : BaseApiController
    {
        private readonly HealthCheckService _healthCheckService;

        public HealthController(
            IServiceProvider serviceProvider,
            ILogger<HealthController> logger) : base(logger)
        {
            _healthCheckService = serviceProvider.GetRequiredService<HealthCheckService>();
        }

        /// <summary>
        /// دریافت وضعیت کلی سلامت برنامه
        /// </summary>
        /// <returns>وضعیت سلامت و جزئیات آن</returns>
        /// <response code="200">سیستم سالم است</response>
        /// <response code="503">سیستم ناسالم است</response>
        [HttpGet]
        [ProducesResponseType(typeof(HealthReport), 200)]
        [ProducesResponseType(typeof(HealthReport), 503)]
        public async Task<IActionResult> GetHealth()
        {
            var report = await _healthCheckService.CheckHealthAsync(reg => reg.Tags.Contains("ready"));
            return report.Status == HealthStatus.Healthy
                ? Success(new { status = "سالم", details = report })
                : Error("سیستم ناسالم است", 503);
        }

        /// <summary>
        /// بررسی زنده بودن برنامه
        /// </summary>
        /// <returns>وضعیت زنده بودن و زمان بررسی</returns>
        /// <response code="200">سیستم زنده است</response>
        [HttpGet("liveness")]
        [ProducesResponseType(typeof(object), 200)]
        public IActionResult GetLiveness()
        {
            return Success(new { status = "زنده", timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// بررسی آمادگی برنامه برای پردازش درخواست‌ها
        /// </summary>
        /// <returns>وضعیت آمادگی و زمان بررسی</returns>
        /// <response code="200">سیستم آماده است</response>
        /// <response code="503">سیستم آماده نیست</response>
        [HttpGet("readiness")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 503)]
        public async Task<IActionResult> GetReadiness()
        {
            var report = await _healthCheckService.CheckHealthAsync(reg => reg.Tags.Contains("ready"));
            return report.Status == HealthStatus.Healthy
                ? Success(new { status = "آماده", timestamp = DateTime.UtcNow })
                : Error("سیستم آماده نیست", 503);
        }

        /// <summary>
        /// دریافت جزئیات وضعیت سلامت
        /// </summary>
        /// <returns>جزئیات وضعیت سلامت تمام اجزا</returns>
        /// <response code="200">جزئیات وضعیت سلامت</response>
        [HttpGet("details")]
        [ProducesResponseType(typeof(HealthReport), 200)]
        public async Task<IActionResult> GetHealthDetails()
        {
            var report = await _healthCheckService.CheckHealthAsync();
            return Success(new { status = report.Status.ToString(), details = report });
        }

        /// <summary>
        /// دریافت وضعیت سلامت اجزای خاص
        /// </summary>
        /// <returns>وضعیت سلامت اجزای مشخص شده</returns>
        /// <response code="200">وضعیت سلامت اجزا</response>
        [HttpGet("components")]
        [ProducesResponseType(typeof(HealthReport), 200)]
        public async Task<IActionResult> GetComponentHealth()
        {
            var report = await _healthCheckService.CheckHealthAsync(reg => reg.Tags.Contains("component"));
            return Success(new { status = report.Status.ToString(), components = report });
        }
    }
}