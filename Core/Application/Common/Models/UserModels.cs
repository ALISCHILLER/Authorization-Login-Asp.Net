using System;
using System.Collections.Generic;

namespace Authorization_Login_Asp.Net.Core.Application.Common.Models
{
    /// <summary>
    /// مدل پایه برای اطلاعات کاربر
    /// </summary>
    public abstract class UserBaseDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// مدل DTO کاربر
    /// </summary>
    public class UserDto : UserBaseDto
    {
        public ICollection<string> Roles { get; set; } = new List<string>();
        public ICollection<string> Permissions { get; set; } = new List<string>();
    }

    /// <summary>
    /// مدل DTO پروفایل کاربر
    /// </summary>
    public class UserProfileDto : UserBaseDto
    {
        public ICollection<RoleDto> Roles { get; set; } = new List<RoleDto>();
    }

    /// <summary>
    /// مدل DTO تنظیمات امنیتی کاربر
    /// </summary>
    public class UserSecuritySettingsDto
    {
        public bool HasTwoFactorEnabled { get; set; }
        public DateTime LastPasswordChangeDate { get; set; }
        public bool RequiresPasswordChange { get; set; }
    }

    /// <summary>
    /// مدل پایه برای ایجاد و به‌روزرسانی کاربر
    /// </summary>
    public abstract class UserModificationDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// مدل DTO کاربر جدید
    /// </summary>
    public class CreateUserDto : UserModificationDto
    {
    }

    /// <summary>
    /// مدل DTO به‌روزرسانی کاربر
    /// </summary>
    public class UpdateUserDto : UserModificationDto
    {
    }
} 