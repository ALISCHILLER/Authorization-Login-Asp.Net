using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;

namespace Authorization_Login_Asp.Net.Core.Presentation.Api.Controllers
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
        protected readonly IMediator _mediator;

        protected BaseApiController(ILogger logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
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
            if (roles == null || roles.Length == 0)
                return false;

            foreach (var role in roles)
            {
                if (User.IsInRole(role))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// اجرای ایمن دستور با مدیریت خطا
        /// </summary>
        protected async Task<IActionResult> ExecuteCommand<T>(IRequest<T> command, string errorMessage = null)
        {
            try
            {
                var result = await _mediator.Send(command);
                return Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing command {Command}: {Message}", 
                    command.GetType().Name, ex.Message);
                return Error(errorMessage ?? "خطا در اجرای درخواست");
            }
        }

        /// <summary>
        /// ایجاد پاسخ خطای استاندارد
        /// </summary>
        protected IActionResult Error(string message, int statusCode = 400)
        {
            _logger.LogError("API Error: {Message}", message);
            return StatusCode(statusCode, new 
            { 
                error = message,
                timestamp = DateTime.UtcNow,
                path = HttpContext?.Request?.Path.Value
            });
        }

        /// <summary>
        /// ایجاد پاسخ موفقیت استاندارد
        /// </summary>
        protected IActionResult Success<T>(T data, string message = null)
        {
            return Ok(new 
            { 
                data, 
                message,
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// بررسی اعتبار مدل و برگرداندن خطاهای اعتبارسنجی
        /// </summary>
        protected IActionResult ValidateModel()
        {
            if (ModelState.IsValid)
                return null;

            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(new 
            { 
                error = "خطای اعتبارسنجی",
                details = errors,
                timestamp = DateTime.UtcNow
            });
        }
    }
} 