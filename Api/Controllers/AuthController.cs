using Authorization_Login_Asp.Net.Application.DTOs;
using Authorization_Login_Asp.Net.Application.Interfaces;
using Authorization_Login_Asp.Net.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly ITwoFactorAuthenticator _twoFactorAuthenticator;

        public AuthController(
            IUserService userService,
            IJwtTokenGenerator jwtTokenGenerator,
            ITwoFactorAuthenticator twoFactorAuthenticator)
        {
            _userService = userService;
            _jwtTokenGenerator = jwtTokenGenerator;
            _twoFactorAuthenticator = twoFactorAuthenticator;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResult), 200)]
        [ProducesResponseType(typeof(AuthResult), 400)]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            var result = await _userService.RegisterAsync(model);
            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Login with username and password
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(TokenResult), 200)]
        [ProducesResponseType(typeof(AuthResult), 400)]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var result = await _userService.LoginAsync(model);
            if (!result.Succeeded)
                return BadRequest(result);

            if (result.RequiresTwoFactor)
                return Ok(new { requiresTwoFactor = true });

            var tokens = await _jwtTokenGenerator.GenerateTokensAsync(result.User);
            return Ok(tokens);
        }

        /// <summary>
        /// Validate two-factor authentication code
        /// </summary>
        [HttpPost("two-factor")]
        [ProducesResponseType(typeof(TokenResult), 200)]
        [ProducesResponseType(typeof(AuthResult), 400)]
        public async Task<IActionResult> TwoFactor([FromBody] TwoFactorDto model)
        {
            var result = await _userService.ValidateTwoFactorAsync(model);
            if (!result.Succeeded)
                return BadRequest(result);

            var tokens = await _jwtTokenGenerator.GenerateTokensAsync(result.User);
            return Ok(tokens);
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(TokenResult), 200)]
        [ProducesResponseType(typeof(AuthResult), 400)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto model)
        {
            var result = await _userService.RefreshTokenAsync(model);
            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Logout and invalidate refresh token
        /// </summary>
        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirst("sub")?.Value;
            await _userService.LogoutAsync(userId);
            return Ok();
        }

        /// <summary>
        /// Enable two-factor authentication
        /// </summary>
        [Authorize]
        [HttpPost("enable-2fa")]
        [ProducesResponseType(typeof(TwoFactorResult), 200)]
        [ProducesResponseType(typeof(AuthResult), 400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> EnableTwoFactor()
        {
            var userId = User.FindFirst("sub")?.Value;
            var result = await _userService.EnableTwoFactorAsync(userId);
            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Disable two-factor authentication
        /// </summary>
        [Authorize]
        [HttpPost("disable-2fa")]
        [ProducesResponseType(typeof(AuthResult), 200)]
        [ProducesResponseType(typeof(AuthResult), 400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> DisableTwoFactor([FromBody] DisableTwoFactorDto model)
        {
            var userId = User.FindFirst("sub")?.Value;
            var result = await _userService.DisableTwoFactorAsync(userId, model.Code);
            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        [Authorize]
        [HttpGet("profile")]
        [ProducesResponseType(typeof(UserProfileDto), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst("sub")?.Value;
            var profile = await _userService.GetUserProfileAsync(userId);
            return Ok(profile);
        }

        /// <summary>
        /// Update current user profile
        /// </summary>
        [Authorize]
        [HttpPut("profile")]
        [ProducesResponseType(typeof(AuthResult), 200)]
        [ProducesResponseType(typeof(AuthResult), 400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto model)
        {
            var userId = User.FindFirst("sub")?.Value;
            var result = await _userService.UpdateProfileAsync(userId, model);
            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }
    }
}