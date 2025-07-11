using System.ComponentModel.DataAnnotations;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Common;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Auth
{
    /// <summary>
    /// Represents the request to change a user's password.
    /// </summary>
    public class ChangePasswordRequest // Removed inheritance from AuthRequest
    {
        /// <summary>
        /// The current password of the user.
        /// </summary>
        [Required(ErrorMessage = "Current password is required.")]
        public string CurrentPassword { get; set; }

        /// <summary>
        /// The new password.
        /// </summary>
        [Required(ErrorMessage = "New password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, one digit, and one special character.")]
        public string NewPassword { get; set; }

        /// <summary>
        /// Confirmation of the new password.
        /// </summary>
        [Required(ErrorMessage = "Confirm new password is required.")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmNewPassword { get; set; }

        /// <summary>
        /// Information about the device making the request.
        /// </summary>
        [Required(ErrorMessage = "Device information is required.")]
        public DeviceInfoDto DeviceInfo { get; set; }

        /// <summary>
        /// Location from where the request is made.
        /// </summary>
        public LocationDto? Location { get; set; } // Made nullable as it's not always required

        /// <summary>
        /// Indicates if the password change is mandatory (e.g., after a reset or first login).
        /// </summary>
        public bool RequirePasswordChange { get; set; }
    }
} 