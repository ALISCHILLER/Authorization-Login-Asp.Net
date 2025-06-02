using Authorization_Login_Asp.Net.Core.Application.DTOs;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediatR;
using Authorization_Login_Asp.Net.Core.Infrastructure.Security;
using Authorization_Login_Asp.Net.Core.Application.Features.Auth.Commands;
using Authorization_Login_Asp.Net.Core.Application.Features.Auth.Queries;

namespace Authorization_Login_Asp.Net.Presentation.Api.Controllers
{
    /// <summary>
    /// کنترلر مدیریت احراز هویت و پروفایل کاربران
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly ITwoFactorAuthenticator _twoFactorAuthenticator;
        private readonly IMediator _mediator;
        private readonly ITwoFactorService _twoFactorService;
        private readonly IPasswordService _passwordService;
        private readonly IDeviceManagementService _deviceService;
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// سازنده کنترلر احراز هویت
        /// </summary>
        /// <param name="userService">سرویس مدیریت کاربران</param>
        /// <param name="jwtTokenGenerator">سرویس تولید توکن JWT</param>
        /// <param name="twoFactorAuthenticator">سرویس احراز هویت دو مرحله‌ای</param>
        /// <param name="mediator">مدیتور</param>
        /// <param name="twoFactorService">سرویس مدیریت احراز هویت دو مرحله‌ای</param>
        /// <param name="passwordService">سرویس مدیریت رمز عبور</param>
        /// <param name="deviceService">سرویس مدیریت دستگاه</param>
        /// <param name="logger">لاگر</param>
        public AuthController(
            IUserService userService,
            IJwtTokenGenerator jwtTokenGenerator,
            ITwoFactorAuthenticator twoFactorAuthenticator,
            IMediator mediator,
            ITwoFactorService twoFactorService,
            IPasswordService passwordService,
            IDeviceManagementService deviceService,
            ILogger<AuthController> logger) : base(logger)
        {
            _userService = userService;
            _jwtTokenGenerator = jwtTokenGenerator;
            _twoFactorAuthenticator = twoFactorAuthenticator;
            _mediator = mediator;
            _twoFactorService = twoFactorService;
            _passwordService = passwordService;
            _deviceService = deviceService;
        }

        #region احراز هویت
        /// <summary>
        /// ثبت‌نام کاربر جدید
        /// </summary>
        /// <param name="command">دستور ثبت‌نام کاربر</param>
        /// <returns>نتیجه ثبت‌نام و توکن‌های دسترسی</returns>
        /// <response code="201">ثبت‌نام با موفقیت انجام شد</response>
        /// <response code="400">اطلاعات ورودی نامعتبر است</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponse), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                return CreatedAtAction(nameof(Login), new { username = command.Username }, result);
            }
            catch (Exception ex)
            {
                return Error("خطا در ثبت‌نام کاربر");
            }
        }

        /// <summary>
        /// ورود با نام کاربری و رمز عبور
        /// </summary>
        /// <param name="command">دستور ورود کاربر</param>
        /// <returns>نتیجه ورود و توکن‌های دسترسی</returns>
        /// <response code="200">ورود با موفقیت انجام شد</response>
        /// <response code="400">اطلاعات ورودی نامعتبر است</response>
        /// <response code="401">کاربر احراز هویت نشده است</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                return Success(result);
            }
            catch (Exception ex)
            {
                return Error("نام کاربری یا رمز عبور اشتباه است", 401);
            }
        }

        /// <summary>
        /// تأیید کد احراز هویت دو مرحله‌ای
        /// </summary>
        /// <param name="command">کد تأیید و اطلاعات کاربر</param>
        /// <returns>نتیجه تأیید و توکن‌های دسترسی</returns>
        /// <response code="200">تأیید با موفقیت انجام شد</response>
        /// <response code="400">اطلاعات ورودی نامعتبر است</response>
        [HttpPost("two-factor")]
        [ProducesResponseType(typeof(AuthResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> TwoFactor([FromBody] ValidateTwoFactorCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _mediator.Send(command);
            return Success(result);
        }

        /// <summary>
        /// تمدید توکن دسترسی با استفاده از توکن رفرش
        /// </summary>
        /// <param name="command">توکن رفرش</param>
        /// <returns>توکن‌های جدید دسترسی</returns>
        /// <response code="200">تمدید با موفقیت انجام شد</response>
        /// <response code="400">توکن رفرش نامعتبر است</response>
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(AuthResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _mediator.Send(command);
            return Success(result);
        }

        /// <summary>
        /// خروج و باطل کردن توکن رفرش
        /// </summary>
        /// <returns>پیام موفقیت‌آمیز بودن خروج</returns>
        /// <response code="200">خروج با موفقیت انجام شد</response>
        /// <response code="401">کاربر احراز هویت نشده است</response>
        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Logout()
        {
            if (!TryGetUserId(out Guid userId))
                return Error("شناسه کاربر نامعتبر است");

            await _mediator.Send(new LogoutCommand { UserId = userId });
            return Success("خروج با موفقیت انجام شد");
        }
        #endregion

        #region احراز هویت دو مرحله‌ای
        /// <summary>
        /// فعال‌سازی احراز هویت دو مرحله‌ای
        /// </summary>
        /// <returns>اطلاعات مورد نیاز برای راه‌اندازی</returns>
        /// <response code="200">فعال‌سازی با موفقیت انجام شد</response>
        /// <response code="400">عملیات ناموفق بود</response>
        /// <response code="401">کاربر احراز هویت نشده است</response>
        [Authorize]
        [HttpPost("enable-2fa")]
        [ProducesResponseType(typeof(TwoFactorSetupResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> EnableTwoFactor()
        {
            if (!TryGetUserId(out Guid userId))
                return Error("شناسه کاربر نامعتبر است");

            var result = await _mediator.Send(new EnableTwoFactorCommand { UserId = userId });
            return Success(result);
        }

        /// <summary>
        /// غیرفعال‌سازی احراز هویت دو مرحله‌ای
        /// </summary>
        /// <param name="command">کد تأیید</param>
        /// <returns>نتیجه غیرفعال‌سازی</returns>
        /// <response code="200">غیرفعال‌سازی با موفقیت انجام شد</response>
        /// <response code="400">کد تأیید نامعتبر است</response>
        /// <response code="401">کاربر احراز هویت نشده است</response>
        [Authorize]
        [HttpPost("disable-2fa")]
        [ProducesResponseType(typeof(AuthResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> DisableTwoFactor([FromBody] DisableTwoFactorCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!TryGetUserId(out Guid userId))
                return Error("شناسه کاربر نامعتبر است");

            command.UserId = userId;
            var result = await _mediator.Send(command);
            return Success(result);
        }
        #endregion

        #region مدیریت پروفایل
        /// <summary>
        /// دریافت پروفایل کاربر جاری
        /// </summary>
        /// <returns>اطلاعات پروفایل کاربر</returns>
        /// <response code="200">اطلاعات پروفایل با موفقیت دریافت شد</response>
        /// <response code="401">کاربر احراز هویت نشده است</response>
        /// <response code="404">کاربر یافت نشد</response>
        [Authorize]
        [HttpGet("profile")]
        [ProducesResponseType(typeof(UserResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProfile()
        {
            if (!TryGetUserId(out Guid userId))
                return Error("شناسه کاربر نامعتبر است");

            var result = await _mediator.Send(new GetUserProfileQuery { UserId = userId });
            return Success(result);
        }

        /// <summary>
        /// به‌روزرسانی پروفایل کاربر جاری
        /// </summary>
        /// <param name="command">اطلاعات جدید پروفایل</param>
        /// <returns>اطلاعات به‌روز شده پروفایل</returns>
        /// <response code="200">پروفایل با موفقیت به‌روز شد</response>
        /// <response code="400">اطلاعات ورودی نامعتبر است</response>
        /// <response code="401">کاربر احراز هویت نشده است</response>
        /// <response code="404">کاربر یافت نشد</response>
        [Authorize]
        [HttpPut("profile")]
        [ProducesResponseType(typeof(UserResponse), 200)]
        [ProducesResponseType(typeof(UserResponse), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!TryGetUserId(out Guid userId))
                return Error("شناسه کاربر نامعتبر است");

            command.UserId = userId;
            var result = await _mediator.Send(command);
            return Success(result);
        }
        #endregion
    }
}