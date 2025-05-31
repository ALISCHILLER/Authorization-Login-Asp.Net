namespace Authorization_Login_Asp.Net.Domain.Interfaces
{
    /// <summary>
    /// رابط برای دسترسی به اطلاعات کاربر فعلی
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>
        /// شناسه کاربر فعلی
        /// </summary>
        int? UserId { get; }

        /// <summary>
        /// نام کاربری کاربر فعلی
        /// </summary>
        string Username { get; }

        /// <summary>
        /// وضعیت احراز هویت کاربر
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// نقش کاربر فعلی
        /// </summary>
        string Role { get; }

        /// <summary>
        /// ایمیل کاربر فعلی
        /// </summary>
        string Email { get; }

        /// <summary>
        /// بررسی دسترسی کاربر به یک پرمیشن خاص
        /// </summary>
        /// <param name="permission">نام پرمیشن</param>
        /// <returns>نتیجه بررسی</returns>
        bool HasPermission(string permission);

        /// <summary>
        /// بررسی دسترسی کاربر به یک نقش خاص
        /// </summary>
        /// <param name="role">نام نقش</param>
        /// <returns>نتیجه بررسی</returns>
        bool IsInRole(string role);
    }
} 