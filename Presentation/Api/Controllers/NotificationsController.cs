using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Application.DTOs;
using Authorization_Login_Asp.Net.Core.Infrastructure.Services;
using System.ComponentModel.DataAnnotations;

namespace Authorization_Login_Asp.Net.Presentation.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "RequireAdminRole")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            INotificationService notificationService,
            ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// دریافت لیست اعلان‌ها
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<NotificationDto>), 200)]
        public async Task<IActionResult> GetNotifications([FromQuery] int count = 10)
        {
            var notifications = await _notificationService.GetNotificationsAsync(count);
            return Ok(notifications);
        }

        /// <summary>
        /// ایجاد اعلان جدید
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(NotificationDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateNotification([FromBody] NotificationRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var notification = await _notificationService.CreateNotificationAsync(request);
            return CreatedAtAction(nameof(GetNotifications), new { id = notification.Id }, notification);
        }

        /// <summary>
        /// ارسال اعلان سیستمی
        /// </summary>
        [HttpPost("system")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> SendSystemAlert([FromBody] AlertRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _notificationService.SendSystemAlertAsync(request.Title, request.Message, request.Severity);
            return Ok(new { message = "اعلان سیستمی با موفقیت ارسال شد" });
        }

        /// <summary>
        /// ارسال اعلان امنیتی
        /// </summary>
        [HttpPost("security")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> SendSecurityAlert([FromBody] AlertRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _notificationService.SendSecurityAlertAsync(request.Title, request.Message, request.Severity);
            return Ok(new { message = "اعلان امنیتی با موفقیت ارسال شد" });
        }

        /// <summary>
        /// ارسال اعلان عملکردی
        /// </summary>
        [HttpPost("performance")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> SendPerformanceAlert([FromBody] AlertRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _notificationService.SendPerformanceAlertAsync(request.Title, request.Message, request.Severity);
            return Ok(new { message = "اعلان عملکردی با موفقیت ارسال شد" });
        }

        /// <summary>
        /// ارسال اعلان خطا
        /// </summary>
        [HttpPost("error")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> SendErrorAlert([FromBody] AlertRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _notificationService.SendErrorAlertAsync(request.Title, request.Message, request.Severity);
            return Ok(new { message = "اعلان خطا با موفقیت ارسال شد" });
        }

        /// <summary>
        /// علامت‌گذاری اعلان به عنوان خوانده شده
        /// </summary>
        [HttpPut("{id}/read")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> MarkAsRead(string id)
        {
            if (!Guid.TryParse(id, out var notificationId))
                return BadRequest("شناسه اعلان نامعتبر است");

            await _notificationService.MarkAsReadAsync(notificationId);
            return Ok(new { message = "اعلان با موفقیت به عنوان خوانده شده علامت‌گذاری شد" });
        }

        /// <summary>
        /// حذف اعلان
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteNotification(string id)
        {
            if (!Guid.TryParse(id, out var notificationId))
                return BadRequest("شناسه اعلان نامعتبر است");

            await _notificationService.DeleteNotificationAsync(notificationId);
            return Ok(new { message = "اعلان با موفقیت حذف شد" });
        }
    }

    public class AlertRequest
    {
        [Required(ErrorMessage = "عنوان اعلان الزامی است")]
        [StringLength(100, ErrorMessage = "عنوان اعلان نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public string Title { get; set; }

        [Required(ErrorMessage = "متن اعلان الزامی است")]
        [StringLength(500, ErrorMessage = "متن اعلان نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        public string Message { get; set; }

        [Required(ErrorMessage = "سطح اهمیت اعلان الزامی است")]
        public AlertSeverity Severity { get; set; }
    }
}