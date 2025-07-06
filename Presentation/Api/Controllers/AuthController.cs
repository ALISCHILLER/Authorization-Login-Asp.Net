using Authorization_Login_Asp.Net.Core.Application.DTOs;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediatR;
using Authorization_Login_Asp.Net.Core.Infrastructure.Security;

namespace Authorization_Login_Asp.Net.Core.Presentation.Api.Controllers
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
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly ITwoFactorAuthenticator _twoFactorAuthenticator;
        private readonly ITwoFactorService _twoFactorService;
        private readonly IPasswordService _passwordService;
        private readonly IDeviceManagementService _deviceService;

        /// <summary>
        /// سازنده کنترلر احراز هویت
        /// </summary>
        public AuthController(
            IUserService userService,
            IJwtTokenGenerator jwtTokenGenerator,
            ITwoFactorAuthenticator twoFactorAuthenticator,
            IMediator mediator,
            ITwoFactorService twoFactorService,
            IPasswordService passwordService,
            IDeviceManagementService deviceService,
            ILogger<AuthController> logger) 
            : base(logger, mediator)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _jwtTokenGenerator = jwtTokenGenerator ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));
            _twoFactorAuthenticator = twoFactorAuthenticator ?? throw new ArgumentNullException(nameof(twoFactorAuthenticator));
            _twoFactorService = twoFactorService ?? throw new ArgumentNullException(nameof(twoFactorService));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
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
            var validationResult = ValidateModel();
            if (validationResult != null)
                return validationResult;

            var result = await ExecuteCommand(command, "خطا در ثبت‌نام کاربر");
            if (result is OkObjectResult okResult)
            {
                return CreatedAtAction(nameof(Login), new { username = command.Username }, okResult.Value);
            }
            return result;
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
            var validationResult = ValidateModel();
            if (validationResult != null)
                return validationResult;

            return await ExecuteCommand(command, "نام کاربری یا رمز عبور اشتباه است");
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
            var validationResult = ValidateModel();
            if (validationResult != null)
                return validationResult;

            return await ExecuteCommand(command, "کد تأیید نامعتبر است");
        }

        /// <summary>
        /// تمدید توکن دسترسی با استفاده از توکن رفرش
        /// </summary>
        /// <param name="command">توکن رفرش</param>
        /// <returns>توکن‌های جدید دسترسی</returns>
        /// <response code="200">تمدید با موفقیت انجام شد</response>
        /// <response code="400">توکن رفرش نامعتبر است</response>
        /// <response code="401">کاربر احراز هویت نشده است</response>
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(AuthResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
        {
            var validationResult = ValidateModel();
            if (validationResult != null)
                return validationResult;

            return await ExecuteCommand(command, "توکن نامعتبر است");
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

            await ExecuteCommand(new LogoutCommand { UserId = userId });
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

            return await ExecuteCommand(new EnableTwoFactorCommand { UserId = userId });
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
            return await ExecuteCommand(command);
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
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProfile()
        {
            if (!TryGetUserId(out Guid userId))
                return Error("شناسه کاربر نامعتبر است");

            return await ExecuteCommand(new GetUserProfileQuery { UserId = userId });
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
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(typeof(UserDto), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command)
        {
            var validationResult = ValidateModel();
            if (validationResult != null)
                return validationResult;

            if (!TryGetUserId(out Guid userId))
                return Error("شناسه کاربر نامعتبر است");

            command.UserId = userId;
            return await ExecuteCommand(command, "خطا در به‌روزرسانی پروفایل");
        }

        /// <summary>
        /// تغییر رمز عبور
        /// </summary>
        [Authorize]
        [HttpPost("change-password")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
        {
            var validationResult = ValidateModel();
            if (validationResult != null)
                return validationResult;

            if (!TryGetUserId(out Guid userId))
                return Error("شناسه کاربر نامعتبر است");

            command.UserId = userId;
            return await ExecuteCommand(command, "خطا در تغییر رمز عبور");
        }
        #endregion
    }
}