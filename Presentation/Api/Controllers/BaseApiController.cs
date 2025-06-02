using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;

namespace Authorization_Login_Asp.Net.Presentation.Api.Controllers
{
    /// <summary>
    /// کلاس پایه برای کنترلرهای API
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public abstract class BaseApiController : ControllerBase
    {
        protected readonly ILogger _logger;

        protected BaseApiController(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// استخراج شناسه کاربر از توکن
        /// </summary>
        protected bool TryGetUserId(out Guid userId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out userId);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به نقش‌های مشخص شده
        /// </summary>
        protected bool HasRole(params string[] roles)
        {
            foreach (var role in roles)
            {
                if (User.IsInRole(role))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// ایجاد پاسخ خطای استاندارد
        /// </summary>
        protected IActionResult Error(string message, int statusCode = 400)
        {
            _logger.LogError(message);
            return StatusCode(statusCode, new { error = message });
        }

        /// <summary>
        /// ایجاد پاسخ موفقیت استاندارد
        /// </summary>
        protected IActionResult Success<T>(T data, string message = null)
        {
            return Ok(new { data, message });
        }

        /// <summary>
        /// ایجاد پاسخ موفقیت با پیام
        /// </summary>
        protected IActionResult Success(string message)
        {
            return Ok(new { message });
        }
    }
} 