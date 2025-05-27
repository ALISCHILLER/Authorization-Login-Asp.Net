using System.Security.Claims;
using Authorization_Login_Asp.Net.Domain.Entities;

namespace Authorization_Login_Asp.Net.Application.Interfaces
{
    /// <summary>
    /// اینترفیس مدیریت توکن JWT
    /// </summary>
    public interface IJwtTokenGenerator
    {
        /// <summary>
        /// تولید توکن JWT برای کاربر
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
    }
} 