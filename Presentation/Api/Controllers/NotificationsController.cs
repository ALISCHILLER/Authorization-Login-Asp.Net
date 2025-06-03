using Authorization_Login_Asp.Net.Core.Application.Features.Notifications.Commands;
using Authorization_Login_Asp.Net.Core.Application.Features.Notifications.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Authorization_Login_Asp.Net.Presentation.Api.Controllers
{
    /// <summary>
    /// کنترلر مدیریت اعلان‌ها و هشدارهای سیستم
    /// </summary>
    [Authorize(Policy = "RequireAdminRole")]
    public class NotificationsController : BaseApiController
    {
        private readonly IMediator _mediator;

        public NotificationsController(
            IMediator mediator,
            ILogger<NotificationsController> logger) : base(logger)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// دریافت لیست اعلان‌ها
        /// </summary>
        /// <param name="count">تعداد اعلان‌های مورد نظر</param>
        /// <returns>لیست اعلان‌ها</returns>
        /// <response code="200">لیست اعلان‌ها با موفقیت دریافت شد</response>
        [HttpGet]
        [ProducesResponseType(typeof(GetNotificationsResponse), 200)]
        public async Task<IActionResult> GetNotifications([FromQuery] int count = 10)
        {
            var result = await _mediator.Send(new GetNotificationsQuery { Count = count });
            return Success(result);
        }

        /// <summary>
        /// ایجاد اعلان جدید
        /// </summary>
        /// <param name="command">اطلاعات اعلان جدید</param>
        /// <returns>اطلاعات اعلان ایجاد شده</returns>
        /// <response code="201">اعلان با موفقیت ایجاد شد</response>
        /// <response code="400">اطلاعات ورودی نامعتبر است</response>
        [HttpPost]
        [ProducesResponseType(typeof(CreateNotificationResponse), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetNotifications), new { id = result.Id }, result);
        }

        /// <summary>
        /// ارسال اعلان سیستمی
        /// </summary>
        /// <param name="command">اطلاعات اعلان سیستمی</param>
        /// <returns>نتیجه ارسال اعلان</returns>
        /// <response code="200">اعلان با موفقیت ارسال شد</response>
        /// <response code="400">اطلاعات ورودی نامعتبر است</response>
        [HttpPost("system")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> SendSystemAlert([FromBody] SendSystemAlertCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _mediator.Send(command);
            return Success("اعلان سیستمی با موفقیت ارسال شد");
        }

        /// <summary>
        /// ارسال اعلان امنیتی
        /// </summary>
        /// <param name="command">اطلاعات اعلان امنیتی</param>
        /// <returns>نتیجه ارسال اعلان</returns>
        /// <response code="200">اعلان با موفقیت ارسال شد</response>
        /// <response code="400">اطلاعات ورودی نامعتبر است</response>
        [HttpPost("security")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> SendSecurityAlert([FromBody] SendSecurityAlertCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _mediator.Send(command);
            return Success("اعلان امنیتی با موفقیت ارسال شد");
        }

        /// <summary>
        /// ارسال اعلان عملکردی
        /// </summary>
        /// <param name="command">اطلاعات اعلان عملکردی</param>
        /// <returns>نتیجه ارسال اعلان</returns>
        /// <response code="200">اعلان با موفقیت ارسال شد</response>
        /// <response code="400">اطلاعات ورودی نامعتبر است</response>
        [HttpPost("performance")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> SendPerformanceAlert([FromBody] SendPerformanceAlertCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _mediator.Send(command);
            return Success("اعلان عملکردی با موفقیت ارسال شد");
        }

        /// <summary>
        /// ارسال اعلان خطا
        /// </summary>
        /// <param name="command">اطلاعات اعلان خطا</param>
        /// <returns>نتیجه ارسال اعلان</returns>
        /// <response code="200">اعلان با موفقیت ارسال شد</response>
        /// <response code="400">اطلاعات ورودی نامعتبر است</response>
        [HttpPost("error")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> SendErrorAlert([FromBody] SendErrorAlertCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _mediator.Send(command);
            return Success("اعلان خطا با موفقیت ارسال شد");
        }

        /// <summary>
        /// علامت‌گذاری اعلان به عنوان خوانده شده
        /// </summary>
        /// <param name="id">شناسه اعلان</param>
        /// <returns>نتیجه عملیات</returns>
        /// <response code="200">اعلان با موفقیت علامت‌گذاری شد</response>
        /// <response code="400">شناسه اعلان نامعتبر است</response>
        /// <response code="404">اعلان یافت نشد</response>
        [HttpPut("{id}/read")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> MarkAsRead(string id)
        {
            if (!Guid.TryParse(id, out var notificationId))
                return Error("شناسه اعلان نامعتبر است");

            await _mediator.Send(new MarkNotificationAsReadCommand { NotificationId = notificationId });
            return Success("اعلان با موفقیت به عنوان خوانده شده علامت‌گذاری شد");
        }

        /// <summary>
        /// حذف اعلان
        /// </summary>
        /// <param name="id">شناسه اعلان</param>
        /// <returns>نتیجه عملیات</returns>
        /// <response code="200">اعلان با موفقیت حذف شد</response>
        /// <response code="400">شناسه اعلان نامعتبر است</response>
        /// <response code="404">اعلان یافت نشد</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteNotification(string id)
        {
            if (!Guid.TryParse(id, out var notificationId))
                return Error("شناسه اعلان نامعتبر است");

            await _mediator.Send(new DeleteNotificationCommand { NotificationId = notificationId });
            return Success("اعلان با موفقیت حذف شد");
        }
    }
}