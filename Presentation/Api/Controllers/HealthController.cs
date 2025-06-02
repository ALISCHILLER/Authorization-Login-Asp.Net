using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Authorization_Login_Asp.Net.Presentation.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class HealthController : ControllerBase
    {
        private readonly HealthCheckService _healthCheckService;

        public HealthController(IServiceProvider serviceProvider)
        {
            _healthCheckService = serviceProvider.GetRequiredService<HealthCheckService>();
        }

        /// <summary>
        /// دریافت وضعیت کلی سلامت برنامه
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(HealthReport), 200)]
        [ProducesResponseType(typeof(HealthReport), 503)]
        public async Task<IActionResult> GetHealth()
        {
            var report = await _healthCheckService.CheckHealthAsync(reg => reg.Tags.Contains("ready"));

            return report.Status == HealthStatus.Healthy
                ? Ok(new { status = "سالم", details = report })
                : StatusCode(503, new { status = "ناسالم", details = report });
        }

        /// <summary>
        /// بررسی زنده بودن برنامه
        /// </summary>
        [HttpGet("liveness")]
        [ProducesResponseType(typeof(string), 200)]
        public IActionResult GetLiveness()
        {
            return Ok(new { status = "زنده", timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// بررسی آمادگی برنامه برای پردازش درخواست‌ها
        /// </summary>
        [HttpGet("readiness")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 503)]
        public async Task<IActionResult> GetReadiness()
        {
            var report = await _healthCheckService.CheckHealthAsync(reg => reg.Tags.Contains("ready"));

            return report.Status == HealthStatus.Healthy
                ? Ok(new { status = "آماده", timestamp = DateTime.UtcNow })
                : StatusCode(503, new { status = "غیر آماده", timestamp = DateTime.UtcNow, details = report });
        }

        /// <summary>
        /// دریافت جزئیات وضعیت سلامت
        /// </summary>
        [HttpGet("details")]
        [ProducesResponseType(typeof(HealthReport), 200)]
        public async Task<IActionResult> GetHealthDetails()
        {
            var report = await _healthCheckService.CheckHealthAsync();
            return Ok(new { status = report.Status.ToString(), details = report });
        }

        /// <summary>
        /// دریافت وضعیت سلامت اجزای خاص
        /// </summary>
        [HttpGet("components")]
        [ProducesResponseType(typeof(HealthReport), 200)]
        public async Task<IActionResult> GetComponentHealth()
        {
            var report = await _healthCheckService.CheckHealthAsync(reg => reg.Tags.Contains("component"));
            return Ok(new { status = report.Status.ToString(), components = report });
        }
    }
}