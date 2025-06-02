using Authorization_Login_Asp.Net.Core.Domain.Entities;

namespace Authorization_Login_Asp.Net.Core.Application.Common.Interfaces
{
    /// <summary>
    /// Interface برای سرویس مدیریت هویت
    /// </summary>
    public interface IIdentityService
    {
        /// <summary>
        /// بررسی اعتبار رمز عبور
        /// </summary>
        Task<bool> ValidatePasswordAsync(User user, string password);

        /// <summary>
        /// ایجاد رمز عبور جدید
        /// </summary>
        Task<string> GeneratePasswordHashAsync(string password);

        /// <summary>
        /// بررسی وجود کاربر با نام کاربری یا ایمیل
        /// </summary>
        Task<bool> IsUserExistsAsync(string usernameOrEmail);

        /// <summary>
        /// ایجاد کاربر جدید
        /// </summary>
        Task<User> CreateUserAsync(
            string username,
            string email,
            string password,
            string firstName,
            string lastName,
            bool isActive = true);

        /// <summary>
        /// به‌روزرسانی اطلاعات کاربر
        /// </summary>
        Task UpdateUserAsync(User user);

        /// <summary>
        /// تغییر رمز عبور
        /// </summary>
        Task ChangePasswordAsync(User user, string currentPassword, string newPassword);

        /// <summary>
        /// بازنشانی رمز عبور
        /// </summary>
        Task ResetPasswordAsync(User user, string newPassword);
    }
} 