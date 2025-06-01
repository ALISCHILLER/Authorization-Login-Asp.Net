using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Presentation.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class HealthController : ControllerBase
    {
        private readonly HealthCheckService _healthCheckService;

        public HealthController(HealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        /// <summary>
        /// Get overall health status of the application
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(HealthReport), 200)]
        [ProducesResponseType(typeof(HealthReport), 503)]
        public async Task<IActionResult> GetHealth()
        {
            var report = await _healthCheckService.CheckHealthAsync();

            return report.Status == HealthStatus.Healthy
                ? Ok(report)
                : StatusCode(503, report);
        }

        /// <summary>
        /// Check if the application is alive
        /// </summary>
        [HttpGet("liveness")]
        [ProducesResponseType(typeof(string), 200)]
        public IActionResult GetLiveness()
        {
            return Ok("زنده");
        }

        /// <summary>
        /// Check if the application is ready to handle requests
        /// </summary>
        [HttpGet("readiness")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 503)]
        public async Task<IActionResult> GetReadiness()
        {
            var report = await _healthCheckService.CheckHealthAsync();

            return report.Status == HealthStatus.Healthy
                ? Ok("آماده")
                : StatusCode(503, "غیر آماده");
        }

        /// <summary>
        /// Get detailed health check results
        /// </summary>
        [HttpGet("details")]
        [ProducesResponseType(typeof(HealthReport), 200)]
        public async Task<IActionResult> GetHealthDetails()
        {
            var report = await _healthCheckService.CheckHealthAsync();
            return Ok(report);
        }

        /// <summary>
        /// Get health check results for specific components
        /// </summary>
        [HttpGet("components")]
        [ProducesResponseType(typeof(HealthReport), 200)]
        public async Task<IActionResult> GetComponentHealth()
        {
            var report = await _healthCheckService.CheckHealthAsync(reg => reg.Tags.Contains("component"));
            return Ok(report);
        }
    }
}