using System.ComponentModel.DataAnnotations;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Auth
{
    /// <summary>
    /// Represents the request to refresh an access token.
    /// </summary>
    public class RefreshTokenRequest // Removed inheritance from AuthRequest
    {
        /// <summary>
        /// The refresh token.
        /// </summary>
        [Required(ErrorMessage = "Refresh token is required.")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// The expired access token.
        /// </summary>
        [Required(ErrorMessage = "Expired access token is required.")]
        public string ExpiredAccessToken { get; set; }
    }
} 