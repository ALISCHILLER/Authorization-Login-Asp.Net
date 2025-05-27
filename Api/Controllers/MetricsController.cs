using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Authorization_Login_Asp.Net.Infrastructure.Services;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "RequireAdminRole")]
    public class MetricsController : ControllerBase
    {
        private readonly IMetricsService _metricsService;
        private readonly ILogger<MetricsController> _logger;

        public MetricsController(
            IMetricsService metricsService,
            ILogger<MetricsController> logger)
        {
            _metricsService = metricsService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetMetrics()
        {
            try
            {
                var metrics = _metricsService.GetMetricsSnapshot();
                _logger.LogInformation("Metrics retrieved successfully");
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving metrics");
                return StatusCode(500, "Error retrieving metrics");
            }
        }

        [HttpGet("endpoints")]
        public async Task<IActionResult> GetEndpointMetrics()
        {
            try
            {
                var metrics = _metricsService.GetMetricsSnapshot();
                return Ok(metrics.EndpointMetrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving endpoint metrics");
                return StatusCode(500, "Error retrieving endpoint metrics");
            }
        }

        [HttpGet("cache")]
        public async Task<IActionResult> GetCacheMetrics()
        {
            try
            {
                var metrics = _metricsService.GetMetricsSnapshot();
                return Ok(metrics.CacheMetrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cache metrics");
                return StatusCode(500, "Error retrieving cache metrics");
            }
        }

        [HttpGet("database")]
        public async Task<IActionResult> GetDatabaseMetrics()
        {
            try
            {
                var metrics = _metricsService.GetMetricsSnapshot();
                return Ok(metrics.DatabaseMetrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving database metrics");
                return StatusCode(500, "Error retrieving database metrics");
            }
        }

        [HttpGet("external-services")]
        public async Task<IActionResult> GetExternalServiceMetrics()
        {
            try
            {
                var metrics = _metricsService.GetMetricsSnapshot();
                return Ok(metrics.ExternalServiceMetrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving external service metrics");
                return StatusCode(500, "Error retrieving external service metrics");
            }
        }
    }
} 