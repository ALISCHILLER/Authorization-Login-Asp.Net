using Authorization_Login_Asp.Net.Core.Application.DTOs.Auth;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Users;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediatR;

namespace Authorization_Login_Asp.Net.Core.Presentation.Api.Controllers
{
    /// <summary>
    /// Manages user authentication, registration, and profile operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : BaseApiController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator for handling commands and queries.</param>
        /// <param name="logger">The logger for this controller.</param>
        public AuthController(
            IMediator mediator,
            ILogger<AuthController> logger)
            : base(logger, mediator) // Removed dateTimeService from base call
        {
        }

        #region Authentication
        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="command">The registration command.</param>
        /// <returns>Authentication response with access tokens upon successful registration.</returns>
        /// <response code="201">User registered successfully.</response>
        /// <response code="400">Invalid input data.</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command) // Assuming RegisterCommand exists
        {
            var validationResult = ValidateModel();
            if (validationResult != null)
                return validationResult;

            var result = await ExecuteCommand(command, "Error during user registration.");
            if (result is OkObjectResult okResult && okResult.Value is AuthResponse authResponse)
            {
                return CreatedAtAction(nameof(Login), new { username = command.Username }, authResponse);
            }
            return result;
        }

        /// <summary>
        /// Logs in a user with username and password.
        /// </summary>
        /// <param name="command">The login command.</param>
        /// <returns>Authentication response with access tokens upon successful login.</returns>
        /// <response code="200">Login successful.</response>
        /// <response code="400">Invalid input data.</response>
        /// <response code="401">Unauthorized access.</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginCommand command) // Assuming LoginCommand exists
        {
            var validationResult = ValidateModel();
            if (validationResult != null)
                return validationResult;

            return await ExecuteCommand(command, "Invalid username or password.");
        }

        /// <summary>
        /// Verifies the two-factor authentication code.
        /// </summary>
        /// <param name="command">The 2FA verification command.</param>
        /// <returns>Authentication response with access tokens upon successful verification.</returns>
        /// <response code="200">2FA verification successful.</response>
        /// <response code="400">Invalid input data or code.</response>
        [HttpPost("two-factor")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> TwoFactor([FromBody] ValidateTwoFactorCommand command) // Assuming ValidateTwoFactorCommand exists
        {
            var validationResult = ValidateModel();
            if (validationResult != null)
                return validationResult;

            return await ExecuteCommand(command, "Invalid 2FA code.");
        }

        /// <summary>
        /// Refreshes an access token using a refresh token.
        /// </summary>
        /// <param name="command">The refresh token command.</param>
        /// <returns>New authentication response with access tokens.</returns>
        /// <response code="200">Token refresh successful.</response>
        /// <response code="400">Invalid refresh token.</response>
        /// <response code="401">Unauthorized access.</response>
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command) // Assuming RefreshTokenCommand exists
        {
            var validationResult = ValidateModel();
            if (validationResult != null)
                return validationResult;

            return await ExecuteCommand(command, "Invalid refresh token.");
        }

        /// <summary>
        /// Logs out the current user and invalidates the refresh token.
        /// </summary>
        /// <returns>A success message.</returns>
        /// <response code="200">Logout successful.</response>
        /// <response code="401">User is not authenticated.</response>
        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout() // Assuming LogoutCommand exists
        {
            if (!TryGetUserId(out Guid userId))
                return Error("Invalid user identifier.");

            await ExecuteCommand(new LogoutCommand { UserId = userId });
            return Success("Logout successful.");
        }
        #endregion

        #region Two-Factor Authentication
        /// <summary>
        /// Enables two-factor authentication for the current user.
        /// </summary>
        /// <returns>Data required for 2FA setup (e.g., QR code, setup key).</returns>
        /// <response code="200">2FA setup initiated successfully.</response>
        /// <response code="400">Operation failed (e.g., 2FA already enabled).</response>
        /// <response code="401">User is not authenticated.</response>
        [Authorize]
        [HttpPost("enable-2fa")]
        [ProducesResponseType(typeof(TwoFactorSetupResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> EnableTwoFactor() // Assuming EnableTwoFactorCommand exists
        {
            if (!TryGetUserId(out Guid userId))
                return Error("Invalid user identifier.");

            return await ExecuteCommand(new EnableTwoFactorCommand { UserId = userId });
        }

        /// <summary>
        /// Disables two-factor authentication for the current user.
        /// </summary>
        /// <param name="command">The command containing the verification code.</param>
        /// <returns>Result of the deactivation.</returns>
        /// <response code="200">2FA disabled successfully.</response>
        /// <response code="400">Invalid verification code.</response>
        /// <response code="401">User is not authenticated.</response>
        [Authorize]
        [HttpPost("disable-2fa")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DisableTwoFactor([FromBody] DisableTwoFactorCommand command) // Assuming DisableTwoFactorCommand exists
        {
            var validationResult = ValidateModel();
            if (validationResult != null)
                return validationResult;

            if (!TryGetUserId(out Guid userId))
                return Error("Invalid user identifier.");

            command.UserId = userId;
            return await ExecuteCommand(command);
        }
        #endregion

        #region Profile Management
        /// <summary>
        /// Gets the profile of the current user.
        /// </summary>
        /// <returns>The current user's profile information.</returns>
        /// <response code="200">Profile retrieved successfully.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="404">User not found.</response>
        [Authorize]
        [HttpGet("profile")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile() // Assuming GetUserProfileQuery exists
        {
            if (!TryGetUserId(out Guid userId))
                return Error("Invalid user identifier.");

            return await ExecuteCommand(new GetUserProfileQuery { UserId = userId });
        }

        /// <summary>
        /// Updates the profile of the current user.
        /// </summary>
        /// <param name="command">The command with updated profile information.</param>
        /// <returns>The updated user profile.</returns>
        /// <response code="200">Profile updated successfully.</response>
        /// <response code="400">Invalid input data.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="404">User not found.</response>
        [Authorize]
        [HttpPut("profile")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command) // Assuming UpdateProfileCommand exists
        {
            var validationResult = ValidateModel();
            if (validationResult != null)
                return validationResult;

            if (!TryGetUserId(out Guid userId))
                return Error("Invalid user identifier.");

            command.UserId = userId;
            return await ExecuteCommand(command, "Error updating profile.");
        }

        /// <summary>
        /// Changes the current user's password.
        /// </summary>
        /// <param name="command">The change password command.</param>
        /// <response code="200">Password changed successfully.</response>
        /// <response code="400">Invalid input data or current password incorrect.</response>
        /// <response code="401">User is not authenticated.</response>
        [Authorize]
        [HttpPost("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command) // Assuming ChangePasswordCommand exists
        {
            var validationResult = ValidateModel();
            if (validationResult != null)
                return validationResult;

            if (!TryGetUserId(out Guid userId))
                return Error("Invalid user identifier.");

            command.UserId = userId;
            return await ExecuteCommand(command, "Error changing password.");
        }
        #endregion
    }
}