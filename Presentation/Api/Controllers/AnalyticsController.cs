using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Application.DTOs;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Authorization_Login_Asp.Net.Presentation.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    [Produces("application/json")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IMetricsService _metricsService;
        private readonly IReportingService _reportingService;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(
            IMetricsService metricsService,
            IReportingService reportingService,
            ILogger<AnalyticsController> logger)
        {
            _metricsService = metricsService;
            _reportingService = reportingService;
            _logger = logger;
        }

        #region Metrics
        /// <summary>
        /// دریافت متریک‌های کلی سیستم
        /// </summary>
        [HttpGet("metrics")]
        [ProducesResponseType(typeof(SystemMetricsDto), 200)]
        public async Task<IActionResult> GetSystemMetrics()
        {
            var metrics = await _metricsService.GetSystemMetricsAsync();
            return Ok(metrics);
        }

        /// <summary>
        /// دریافت متریک‌های عملکرد API
        /// </summary>
        [HttpGet("metrics/endpoints")]
        [ProducesResponseType(typeof(EndpointMetricsDto), 200)]
        public async Task<IActionResult> GetEndpointMetrics()
        {
            var metrics = await _metricsService.GetEndpointMetricsAsync();
            return Ok(metrics);
        }

        /// <summary>
        /// دریافت متریک‌های پایگاه داده
        /// </summary>
        [HttpGet("metrics/database")]
        [ProducesResponseType(typeof(DatabaseMetricsDto), 200)]
        public async Task<IActionResult> GetDatabaseMetrics()
        {
            var metrics = await _metricsService.GetDatabaseMetricsAsync();
            return Ok(metrics);
        }
        #endregion

        #region Reports
        /// <summary>
        /// دریافت گزارش فعالیت کاربران
        /// </summary>
        [HttpGet("reports/user-activity")]
        [ProducesResponseType(typeof(UserActivityReportDto), 200)]
        public async Task<IActionResult> GetUserActivityReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var report = await _reportingService.GenerateUserActivityReportAsync(startDate, endDate);
            return Ok(report);
        }

        /// <summary>
        /// دریافت گزارش امنیتی
        /// </summary>
        [HttpGet("reports/security")]
        [ProducesResponseType(typeof(SecurityReportDto), 200)]
        public async Task<IActionResult> GetSecurityReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var report = await _reportingService.GenerateSecurityReportAsync(startDate, endDate);
            return Ok(report);
        }

        /// <summary>
        /// دریافت گزارش عملکرد سیستم
        /// </summary>
        [HttpGet("reports/performance")]
        [ProducesResponseType(typeof(PerformanceReportDto), 200)]
        public async Task<IActionResult> GetPerformanceReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var report = await _reportingService.GeneratePerformanceReportAsync(startDate, endDate);
            return Ok(report);
        }

        /// <summary>
        /// دریافت گزارش خطاها
        /// </summary>
        [HttpGet("reports/errors")]
        [ProducesResponseType(typeof(ErrorReportDto), 200)]
        public async Task<IActionResult> GetErrorReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var report = await _reportingService.GenerateErrorReportAsync(startDate, endDate);
            return Ok(report);
        }
        #endregion
    }
} 