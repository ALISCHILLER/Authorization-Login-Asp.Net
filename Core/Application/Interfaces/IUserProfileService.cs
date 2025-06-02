using System;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Application.DTOs;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    /// <summary>
    /// اینترفیس سرویس مدیریت پروفایل کاربر
    /// این اینترفیس عملیات مربوط به مدیریت پروفایل کاربر را تعریف می‌کند
    /// </summary>
    public interface IUserProfileService
    {
        /// <summary>
        /// به‌روزرسانی پروفایل کاربر
        /// </summary>
        Task<bool> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);

        /// <summary>
        /// تغییر رمز عبور
        /// </summary>
        Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);

        /// <summary>
        /// به‌روزرسانی ایمیل
        /// </summary>
        Task<bool> UpdateEmailAsync(Guid userId, string newEmail);

        /// <summary>
        /// به‌روزرسانی شماره تلفن
        /// </summary>
        Task<bool> UpdatePhoneNumberAsync(Guid userId, string newPhoneNumber);

        /// <summary>
        /// به‌روزرسانی تصویر پروفایل
        /// </summary>
        Task<bool> UpdateProfileImageAsync(Guid userId, byte[] imageData);

        /// <summary>
        /// دریافت اطلاعات پروفایل کاربر
        /// </summary>
        Task<UserProfileDto> GetProfileAsync(Guid userId);
    }
} 