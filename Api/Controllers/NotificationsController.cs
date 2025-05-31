using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Authorization_Login_Asp.Net.Infrastructure.Services;
using Authorization_Login_Asp.Net.Application.DTOs;
using System;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Domain.Entities;

namespace Authorization_Login_Asp.Net.Api.Controllers
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

        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] int count = 10)
        {
            try
            {
                var notifications = await _notificationService.GetNotificationsAsync(count);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications");
                return StatusCode(500, "Error getting notifications");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateNotification([FromBody] NotificationRequest request)
        {
            try
            {
                var notification = await _notificationService.CreateNotificationAsync(request);
                return Ok(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification");
                return StatusCode(500, "Error creating notification");
            }
        }

        [HttpPost("system")]
        public async Task<IActionResult> SendSystemAlert(
            [FromBody] AlertRequest request)
        {
            try
            {
                await _notificationService.SendSystemAlertAsync(
                    request.Title,
                    request.Message,
                    request.Severity);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending system alert");
                return StatusCode(500, "Error sending system alert");
            }
        }

        [HttpPost("security")]
        public async Task<IActionResult> SendSecurityAlert(
            [FromBody] AlertRequest request)
        {
            try
            {
                await _notificationService.SendSecurityAlertAsync(
                    request.Title,
                    request.Message,
                    request.Severity);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending security alert");
                return StatusCode(500, "Error sending security alert");
            }
        }

        [HttpPost("performance")]
        public async Task<IActionResult> SendPerformanceAlert(
            [FromBody] AlertRequest request)
        {
            try
            {
                await _notificationService.SendPerformanceAlertAsync(
                    request.Title,
                    request.Message,
                    request.Severity);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending performance alert");
                return StatusCode(500, "Error sending performance alert");
            }
        }

        [HttpPost("error")]
        public async Task<IActionResult> SendErrorAlert(
            [FromBody] AlertRequest request)
        {
            try
            {
                await _notificationService.SendErrorAlertAsync(
                    request.Title,
                    request.Message,
                    request.Severity);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending error alert");
                return StatusCode(500, "Error sending error alert");
            }
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(string id)
        {
            try
            {
                if (!Guid.TryParse(id, out var notificationId))
                {
                    return BadRequest("شناسه اعلان نامعتبر است");
                }

                await _notificationService.MarkAsReadAsync(notificationId);
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return StatusCode(500, "Error marking notification as read");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(string id)
        {
            try
            {
                if (!Guid.TryParse(id, out var notificationId))
                {
                    return BadRequest("شناسه اعلان نامعتبر است");
                }

                await _notificationService.DeleteNotificationAsync(notificationId);
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification");
                return StatusCode(500, "Error deleting notification");
            }
        }
    }

    public class AlertRequest
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public AlertSeverity Severity { get; set; }
    }
} 