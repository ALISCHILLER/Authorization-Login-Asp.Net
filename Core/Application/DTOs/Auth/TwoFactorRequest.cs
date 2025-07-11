using System;
using System.ComponentModel.DataAnnotations;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Common;
using Authorization_Login_Asp.Net.Core.Domain.Enums;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Auth
{
    /// <summary>
    /// Represents the request for two-factor authentication verification.
    /// </summary>
    public class TwoFactorRequest // Removed inheritance from AuthRequest
    {
        /// <summary>
        /// The user's unique identifier.
        /// </summary>
        [Required(ErrorMessage = "User ID is required.")]
        public Guid UserId { get; set; }

        /// <summary>
        /// The verification code.
        /// </summary>
        [Required(ErrorMessage = "Verification code is required.")]
        [RegularExpression(@"^[0-9]{6}$", ErrorMessage = "Verification code must be 6 digits.")]
        public string Code { get; set; }

        /// <summary>
        /// The two-factor authentication provider type.
        /// </summary>
        [Required(ErrorMessage = "Two-factor provider is required.")]
        public TwoFactorType Provider { get; set; }

        /// <summary>
        /// Indicates whether to remember the device for future logins.
        /// </summary>
        public bool RememberDevice { get; set; }

        /// <summary>
        /// Information about the device making the request.
        /// </summary>
        [Required(ErrorMessage = "Device information is required.")]
        public DeviceInfoDto DeviceInfo { get; set; }

        /// <summary>
        /// Location from where the request is made.
        /// </summary>
        public LocationDto? Location { get; set; } // Made nullable
    }
} 