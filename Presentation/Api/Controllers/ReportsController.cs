using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Infrastructure.Services;

namespace Authorization_Login_Asp.Net.Presentation.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "RequireAdminRole")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportingService _reportingService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(
            IReportingService reportingService,
            ILogger<ReportsController> logger)
        {
            _reportingService = reportingService;
            _logger = logger;
        }

        [HttpGet("system")]
        public async Task<IActionResult> GetSystemReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var report = await _reportingService.GenerateSystemReportAsync(startDate, endDate);
                return File(report, "application/json", $"system-report-{DateTime.UtcNow:yyyyMMdd}.json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating system report");
                return StatusCode(500, "Error generating system report");
            }
        }

        [HttpGet("user-activity")]
        public async Task<IActionResult> GetUserActivityReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var report = await _reportingService.GenerateUserActivityReportAsync(startDate, endDate);
                return File(report, "application/json", $"user-activity-report-{DateTime.UtcNow:yyyyMMdd}.json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating user activity report");
                return StatusCode(500, "Error generating user activity report");
            }
        }

        [HttpGet("security")]
        public async Task<IActionResult> GetSecurityReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var report = await _reportingService.GenerateSecurityReportAsync(startDate, endDate);
                return File(report, "application/json", $"security-report-{DateTime.UtcNow:yyyyMMdd}.json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating security report");
                return StatusCode(500, "Error generating security report");
            }
        }

        [HttpGet("performance")]
        public async Task<IActionResult> GetPerformanceReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var report = await _reportingService.GeneratePerformanceReportAsync(startDate, endDate);
                return File(report, "application/json", $"performance-report-{DateTime.UtcNow:yyyyMMdd}.json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating performance report");
                return StatusCode(500, "Error generating performance report");
            }
        }

        [HttpGet("errors")]
        public async Task<IActionResult> GetErrorReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var report = await _reportingService.GenerateErrorReportAsync(startDate, endDate);
                return File(report, "application/json", $"error-report-{DateTime.UtcNow:yyyyMMdd}.json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating error report");
                return StatusCode(500, "Error generating error report");
            }
        }
    }
}