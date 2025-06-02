using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    /// <summary>
    /// رابط برای دسترسی به اطلاعات کاربر فعلی
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>
        /// شناسه کاربر فعلی
        /// </summary>
        Guid? UserId { get; }

        /// <summary>
        /// نام کاربری کاربر فعلی
        /// </summary>
        string Username { get; }

        /// <summary>
        /// وضعیت احراز هویت کاربر
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// نقش‌های کاربر فعلی
        /// </summary>
        string[] Roles { get; }

        /// <summary>
        /// ایمیل کاربر فعلی
        /// </summary>
        string Email { get; }

        /// <summary>
        /// بررسی دسترسی کاربر به یک پرمیشن خاص
        /// </summary>
        /// <param name="permission">نام پرمیشن</param>
        /// <returns>نتیجه بررسی</returns>
        Task<bool> HasPermissionAsync(string permission);

        /// <summary>
        /// بررسی دسترسی کاربر به یک نقش خاص
        /// </summary>
        /// <param name="role">نام نقش</param>
        /// <returns>نتیجه بررسی</returns>
        Task<bool> IsInRoleAsync(string role);

        /// <summary>
        /// دریافت تمام پرمیشن‌های کاربر
        /// </summary>
        /// <returns>لیست پرمیشن‌ها</returns>
        Task<string[]> GetPermissionsAsync();

        /// <summary>
        /// بررسی دسترسی کاربر به چند پرمیشن
        /// </summary>
        /// <param name="permissions">نام پرمیشن‌ها</param>
        /// <returns>نتیجه بررسی</returns>
        Task<bool> HasAnyPermissionAsync(params string[] permissions);

        /// <summary>
        /// بررسی دسترسی کاربر به همه پرمیشن‌ها
        /// </summary>
        /// <param name="permissions">نام پرمیشن‌ها</param>
        /// <returns>نتیجه بررسی</returns>
        Task<bool> HasAllPermissionsAsync(params string[] permissions);
    }
} 