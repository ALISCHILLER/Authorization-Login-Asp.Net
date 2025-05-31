using System.Security.Claims;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Domain.Entities;

namespace Authorization_Login_Asp.Net.Application.Interfaces
{
    /// <summary>
    /// رابط تولید و اعتبارسنجی توکن JWT
    /// </summary>
    public interface IJwtTokenGenerator
    {
        /// <summary>
        /// تولید توکن JWT و توکن بازیابی برای کاربر
        /// </summary>
        /// <param name="user">کاربر</param>
        /// <param name="ipAddress">آدرس IP</param>
        /// <returns>توکن JWT و توکن بازیابی</returns>
        Task<(string Token, string RefreshToken)> GenerateTokensAsync(User user, string ipAddress);

        /// <summary>
        /// تولید توکن JWT
        /// </summary>
        /// <param name="user">کاربر</param>
        /// <returns>توکن JWT</returns>
        string GenerateToken(User user);

        /// <summary>
        /// اعتبارسنجی توکن JWT
        /// </summary>
        /// <param name="token">توکن JWT</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        bool ValidateToken(string token);

        /// <summary>
        /// استخراج اطلاعات کاربر از توکن JWT
        /// </summary>
        /// <param name="token">توکن JWT</param>
        /// <returns>اطلاعات کاربر</returns>
        ClaimsPrincipal GetPrincipalFromToken(string token);

        /// <summary>
        /// لغو توکن JWT
        /// </summary>
        /// <param name="token">توکن JWT</param>
        void RevokeToken(string token);

        /// <summary>
        /// بررسی لغو شدن توکن JWT
        /// </summary>
        /// <param name="token">توکن JWT</param>
        /// <returns>نتیجه بررسی</returns>
        bool IsTokenRevoked(string token);
    }
} 