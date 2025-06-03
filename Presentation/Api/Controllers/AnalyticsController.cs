using Authorization_Login_Asp.Net.Core.Application.Features.Analytics.Commands;
using Authorization_Login_Asp.Net.Core.Application.Features.Analytics.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Presentation.Api.Controllers
{
    /// <summary>
    /// کنترلر مدیریت تحلیل‌ها و گزارش‌های سیستم
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AnalyticsController : BaseApiController
    {
        private readonly IMediator _mediator;

        public AnalyticsController(
            IMediator mediator,
            ILogger<AnalyticsController> logger) : base(logger)
        {
            _mediator = mediator;
        }

        #region متریک‌ها
        /// <summary>
        /// دریافت متریک‌های کلی سیستم
        /// </summary>
        /// <returns>متریک‌های کلی سیستم</returns>
        /// <response code="200">متریک‌های سیستم با موفقیت دریافت شد</response>
        [HttpGet("metrics")]
        [ProducesResponseType(typeof(GetSystemMetricsResponse), 200)]
        public async Task<IActionResult> GetSystemMetrics()
        {
            var result = await _mediator.Send(new GetSystemMetricsQuery());
            return Success(result);
        }

        /// <summary>
        /// دریافت متریک‌های عملکرد API
        /// </summary>
        /// <returns>متریک‌های عملکرد API</returns>
        /// <response code="200">متریک‌های API با موفقیت دریافت شد</response>
        [HttpGet("metrics/endpoints")]
        [ProducesResponseType(typeof(GetEndpointMetricsResponse), 200)]
        public async Task<IActionResult> GetEndpointMetrics()
        {
            var result = await _mediator.Send(new GetEndpointMetricsQuery());
            return Success(result);
        }

        /// <summary>
        /// دریافت متریک‌های پایگاه داده
        /// </summary>
        /// <returns>متریک‌های پایگاه داده</returns>
        /// <response code="200">متریک‌های پایگاه داده با موفقیت دریافت شد</response>
        [HttpGet("metrics/database")]
        [ProducesResponseType(typeof(GetDatabaseMetricsResponse), 200)]
        public async Task<IActionResult> GetDatabaseMetrics()
        {
            var result = await _mediator.Send(new GetDatabaseMetricsQuery());
            return Success(result);
        }
        #endregion

        #region گزارش‌ها
        /// <summary>
        /// دریافت گزارش فعالیت کاربران
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>گزارش فعالیت کاربران</returns>
        /// <response code="200">گزارش با موفقیت دریافت شد</response>
        /// <response code="400">تاریخ‌های ورودی نامعتبر است</response>
        [HttpGet("reports/user-activity")]
        [ProducesResponseType(typeof(GetUserActivityReportResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetUserActivityReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var result = await _mediator.Send(new GetUserActivityReportQuery 
            { 
                StartDate = startDate,
                EndDate = endDate
            });
            return Success(result);
        }

        /// <summary>
        /// دریافت گزارش امنیتی
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>گزارش امنیتی</returns>
        /// <response code="200">گزارش با موفقیت دریافت شد</response>
        /// <response code="400">تاریخ‌های ورودی نامعتبر است</response>
        [HttpGet("reports/security")]
        [ProducesResponseType(typeof(GetSecurityReportResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetSecurityReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var result = await _mediator.Send(new GetSecurityReportQuery 
            { 
                StartDate = startDate,
                EndDate = endDate
            });
            return Success(result);
        }

        /// <summary>
        /// دریافت گزارش عملکرد سیستم
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>گزارش عملکرد سیستم</returns>
        /// <response code="200">گزارش با موفقیت دریافت شد</response>
        /// <response code="400">تاریخ‌های ورودی نامعتبر است</response>
        [HttpGet("reports/performance")]
        [ProducesResponseType(typeof(GetPerformanceReportResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetPerformanceReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var result = await _mediator.Send(new GetPerformanceReportQuery 
            { 
                StartDate = startDate,
                EndDate = endDate
            });
            return Success(result);
        }

        /// <summary>
        /// دریافت گزارش خطاها
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>گزارش خطاها</returns>
        /// <response code="200">گزارش با موفقیت دریافت شد</response>
        /// <response code="400">تاریخ‌های ورودی نامعتبر است</response>
        [HttpGet("reports/errors")]
        [ProducesResponseType(typeof(GetErrorReportResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetErrorReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var result = await _mediator.Send(new GetErrorReportQuery 
            { 
                StartDate = startDate,
                EndDate = endDate
            });
            return Success(result);
        }
        #endregion
    }
} 