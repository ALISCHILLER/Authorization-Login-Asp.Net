using Authorization_Login_Asp.Net.Core.Application.DTOs;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Presentation.Api.Controllers
{
    /// <summary>
    /// کنترلر مدیریت احراز هویت و پروفایل کاربران
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly ITwoFactorAuthenticator _twoFactorAuthenticator;

        /// <summary>
        /// سازنده کنترلر احراز هویت
        /// </summary>
        /// <param name="userService">سرویس مدیریت کاربران</param>
        /// <param name="jwtTokenGenerator">سرویس تولید توکن JWT</param>
        /// <param name="twoFactorAuthenticator">سرویس احراز هویت دو مرحله‌ای</param>
        public AuthController(
            IUserService userService,
            IJwtTokenGenerator jwtTokenGenerator,
            ITwoFactorAuthenticator twoFactorAuthenticator)
        {
            _userService = userService;
            _jwtTokenGenerator = jwtTokenGenerator;
            _twoFactorAuthenticator = twoFactorAuthenticator;
        }

        #region احراز هویت
        /// <summary>
        /// ثبت‌نام کاربر جدید
        /// </summary>
        /// <param name="model">اطلاعات ثبت‌نام کاربر</param>
        /// <returns>نتیجه ثبت‌نام و توکن‌های دسترسی</returns>
        /// <response code="200">ثبت‌نام با موفقیت انجام شد</response>
        /// <response code="400">اطلاعات ورودی نامعتبر است</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponse), 200)]
        [ProducesResponseType(typeof(AuthResponse), 400)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.RegisterAsync(model);
            return Ok(result);
        }

        /// <summary>
        /// ورود با نام کاربری و رمز عبور
        /// </summary>
        /// <param name="model">اطلاعات ورود کاربر</param>
        /// <returns>نتیجه ورود و توکن‌های دسترسی</returns>
        /// <response code="200">ورود با موفقیت انجام شد</response>
        /// <response code="400">اطلاعات ورودی نامعتبر است</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponse), 200)]
        [ProducesResponseType(typeof(AuthResponse), 400)]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.LoginAsync(model);
            return Ok(result);
        }

        /// <summary>
        /// تأیید کد احراز هویت دو مرحله‌ای
        /// </summary>
        /// <param name="model">کد تأیید و اطلاعات کاربر</param>
        /// <returns>نتیجه تأیید و توکن‌های دسترسی</returns>
        /// <response code="200">تأیید با موفقیت انجام شد</response>
        /// <response code="400">اطلاعات ورودی نامعتبر است</response>
        [HttpPost("two-factor")]
        [ProducesResponseType(typeof(AuthResponse), 200)]
        [ProducesResponseType(typeof(AuthResponse), 400)]
        public async Task<IActionResult> TwoFactor([FromBody] TwoFactorDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.ValidateTwoFactorAsync(model);
            return Ok(result);
        }

        /// <summary>
        /// تمدید توکن دسترسی با استفاده از توکن رفرش
        /// </summary>
        /// <param name="model">توکن رفرش</param>
        /// <returns>توکن‌های جدید دسترسی</returns>
        /// <response code="200">تمدید با موفقیت انجام شد</response>
        /// <response code="400">توکن رفرش نامعتبر است</response>
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(AuthResponse), 200)]
        [ProducesResponseType(typeof(AuthResponse), 400)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.RefreshTokenAsync(model);
            return Ok(result);
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
            var userIdClaim = User.FindFirst("sub")?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                return BadRequest("شناسه کاربر نامعتبر است");
            }

            await _userService.LogoutAsync(userId);
            return Ok(new { message = "خروج با موفقیت انجام شد" });
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
        [ProducesResponseType(typeof(AuthResponse), 400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> EnableTwoFactor()
        {
            var userIdClaim = User.FindFirst("sub")?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                return BadRequest("شناسه کاربر نامعتبر است");
            }

            var result = await _userService.EnableTwoFactorAsync(userId);
            return Ok(result);
        }

        /// <summary>
        /// غیرفعال‌سازی احراز هویت دو مرحله‌ای
        /// </summary>
        /// <param name="model">کد تأیید</param>
        /// <returns>نتیجه غیرفعال‌سازی</returns>
        /// <response code="200">غیرفعال‌سازی با موفقیت انجام شد</response>
        /// <response code="400">کد تأیید نامعتبر است</response>
        /// <response code="401">کاربر احراز هویت نشده است</response>
        [Authorize]
        [HttpPost("disable-2fa")]
        [ProducesResponseType(typeof(AuthResponse), 200)]
        [ProducesResponseType(typeof(AuthResponse), 400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> DisableTwoFactor([FromBody] DisableTwoFactorRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst("sub")?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                return BadRequest("شناسه کاربر نامعتبر است");
            }

            var result = await _userService.DisableTwoFactorAsync(userId, model.Code);
            return Ok(result);
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
            var userIdClaim = User.FindFirst("sub")?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                return BadRequest("شناسه کاربر نامعتبر است");
            }

            var profile = await _userService.GetUserByIdAsync(userId);
            if (profile == null)
                return NotFound();

            return Ok(profile);
        }

        /// <summary>
        /// به‌روزرسانی پروفایل کاربر جاری
        /// </summary>
        /// <param name="model">اطلاعات جدید پروفایل</param>
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
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst("sub")?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                return BadRequest("شناسه کاربر نامعتبر است");
            }

            var result = await _userService.UpdateProfileAsync(userId, model);
            if (result == null)
                return NotFound();

            return Ok(result);
        }
        #endregion
    }
}