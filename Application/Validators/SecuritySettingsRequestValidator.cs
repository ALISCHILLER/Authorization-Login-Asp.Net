using FluentValidation;
using Authorization_Login_Asp.Net.Application.DTOs;

namespace Authorization_Login_Asp.Net.Application.Validators
{
    /// <summary>
    /// اعتبارسنجی برای درخواست به‌روزرسانی تنظیمات امنیتی
    /// </summary>
    public class SecuritySettingsRequestValidator : AbstractValidator<SecuritySettingsRequest>
    {
        public SecuritySettingsRequestValidator()
        {
            // اعتبارسنجی تعداد تلاش‌های ناموفق
            RuleFor(x => x.MaxFailedLoginAttempts)
                .InclusiveBetween(3, 10)
                .WithMessage("تعداد تلاش‌های ناموفق باید بین ۳ تا ۱۰ باشد.");

            // اعتبارسنجی مدت زمان قفل شدن
            RuleFor(x => x.AccountLockoutDuration)
                .InclusiveBetween(5, 1440)
                .WithMessage("مدت زمان قفل شدن باید بین ۵ تا ۱۴۴۰ دقیقه باشد.");

            // اعتبارسنجی مدت زمان اعتبار رمز عبور
            RuleFor(x => x.PasswordExpirationDays)
                .InclusiveBetween(30, 365)
                .WithMessage("مدت زمان اعتبار رمز عبور باید بین ۳۰ تا ۳۶۵ روز باشد.");
        }
    }
} 