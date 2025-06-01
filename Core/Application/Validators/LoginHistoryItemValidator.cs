using FluentValidation;
using Authorization_Login_Asp.Net.Core.Application.DTOs;

namespace Authorization_Login_Asp.Net.Core.Application.Validators
{
    /// <summary>
    /// اعتبارسنجی برای آیتم‌های تاریخچه ورود
    /// </summary>
    public class LoginHistoryItemValidator : AbstractValidator<LoginHistoryItem>
    {
        public LoginHistoryItemValidator()
        {
            // اعتبارسنجی شناسه
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("شناسه الزامی است.");

            // اعتبارسنجی شناسه کاربر
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("شناسه کاربر الزامی است.");

            // اعتبارسنجی تاریخ و زمان ورود
            RuleFor(x => x.LoginTime)
                .NotEmpty()
                .WithMessage("تاریخ و زمان ورود الزامی است.");

            // اعتبارسنجی آدرس IP
            RuleFor(x => x.IpAddress)
                .NotEmpty()
                .WithMessage("آدرس IP الزامی است.")
                .Matches(@"^(\d{1,3}\.){3}\d{1,3}$")
                .WithMessage("فرمت آدرس IP معتبر نیست.");

            // اعتبارسنجی اطلاعات مرورگر
            RuleFor(x => x.UserAgent)
                .NotEmpty()
                .WithMessage("اطلاعات مرورگر الزامی است.")
                .MaximumLength(500)
                .WithMessage("اطلاعات مرورگر نمی‌تواند بیشتر از ۵۰۰ کاراکتر باشد.");

            // اعتبارسنجی نام دستگاه
            RuleFor(x => x.DeviceName)
                .MaximumLength(100)
                .WithMessage("نام دستگاه نمی‌تواند بیشتر از ۱۰۰ کاراکتر باشد.");

            // اعتبارسنجی نوع دستگاه
            RuleFor(x => x.DeviceType)
                .MaximumLength(50)
                .WithMessage("نوع دستگاه نمی‌تواند بیشتر از ۵۰ کاراکتر باشد.");

            // اعتبارسنجی سیستم عامل
            RuleFor(x => x.OperatingSystem)
                .MaximumLength(50)
                .WithMessage("سیستم عامل نمی‌تواند بیشتر از ۵۰ کاراکتر باشد.");

            // اعتبارسنجی نام مرورگر
            RuleFor(x => x.BrowserName)
                .MaximumLength(50)
                .WithMessage("نام مرورگر نمی‌تواند بیشتر از ۵۰ کاراکتر باشد.");

            // اعتبارسنجی نسخه مرورگر
            RuleFor(x => x.BrowserVersion)
                .MaximumLength(50)
                .WithMessage("نسخه مرورگر نمی‌تواند بیشتر از ۵۰ کاراکتر باشد.");

            // اعتبارسنجی کشور
            RuleFor(x => x.Country)
                .MaximumLength(100)
                .WithMessage("کشور نمی‌تواند بیشتر از ۱۰۰ کاراکتر باشد.");

            // اعتبارسنجی شهر
            RuleFor(x => x.City)
                .MaximumLength(100)
                .WithMessage("شهر نمی‌تواند بیشتر از ۱۰۰ کاراکتر باشد.");

            // اعتبارسنجی دلیل شکست
            RuleFor(x => x.FailureReason)
                .MaximumLength(200)
                .WithMessage("دلیل شکست نمی‌تواند بیشتر از ۲۰۰ کاراکتر باشد.")
                .NotEmpty()
                .When(x => !x.IsSuccessful)
                .WithMessage("در صورت ناموفق بودن ورود، دلیل شکست الزامی است.");

            // اعتبارسنجی زمان خروج
            RuleFor(x => x.LogoutTime)
                .GreaterThan(x => x.LoginTime)
                .When(x => x.LogoutTime.HasValue)
                .WithMessage("زمان خروج باید بعد از زمان ورود باشد.");

            // اعتبارسنجی مدت زمان نشست
            RuleFor(x => x.SessionDuration)
               .GreaterThan(TimeSpan.Zero)
                .When(x => x.SessionDuration.HasValue)
                .WithMessage("مدت زمان نشست باید بزرگتر از صفر باشد.");
        }
    }
}