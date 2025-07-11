using Authorization_Login_Asp.Net.Core.Application.DTOs.Auth;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Authorization_Login_Asp.Net.Core.Application.Features.Auth.Commands.Login
{
    /// <summary>
    /// Command to log in a user.
    /// </summary>
    public class LoginCommand : IRequest<LoginResponse> // Assuming LoginResponse is the DTO for the login result
    {
        /// <summary>
        /// The username or email of the user.
        /// </summary>
        [Required(ErrorMessage = "Username or email is required.")]
        public string UsernameOrEmail { get; set; }

        /// <summary>
        /// The user's password.
        /// </summary>
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }

        /// <summary>
        /// Indicates whether to remember the user's session.
        /// </summary>
        public bool RememberMe { get; set; }

        /// <summary>
        /// The IP address of the user making the request.
        /// </summary>
        public string? IpAddress { get; set; } // Made nullable as it might not always be available or required

        /// <summary>
        /// The device token, if available (e.g., for push notifications or device identification).
        /// </summary>
        public string? DeviceToken { get; set; } // Made nullable
    }
} 