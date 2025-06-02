using Authorization_Login_Asp.Net.Core.Application.DTOs.Users;

namespace Authorization_Login_Asp.Net.Core.Application.Features.Auth.Commands.Login;

/// <summary>
/// پاسخ دستور ورود کاربر
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// وضعیت موفقیت عملیات
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// توکن دسترسی
    /// </summary>
    public string AccessToken { get; set; }

    /// <summary>
    /// توکن نوسازی
    /// </summary>
    public string RefreshToken { get; set; }

    /// <summary>
    /// زمان انقضای توکن (به ثانیه)
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// نوع توکن
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// پیام خطا در صورت عدم موفقیت
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// اطلاعات کاربر
    /// </summary>
    public UserDto User { get; set; }

    /// <summary>
    /// زمان آخرین ورود موفق
    /// </summary>
    public DateTime? LastSuccessfulLogin { get; set; }

    /// <summary>
    /// تعداد تلاش‌های ناموفق اخیر
    /// </summary>
    public int RecentFailedAttempts { get; set; }

    /// <summary>
    /// زمان قفل شدن حساب (در صورت وجود)
    /// </summary>
    public DateTime? AccountLockoutEnd { get; set; }
} 