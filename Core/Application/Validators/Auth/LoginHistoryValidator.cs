using FluentValidation;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Auth;
using Authorization_Login_Asp.Net.Core.Application.Validators.Common;

namespace Authorization_Login_Asp.Net.Core.Application.Validators.Auth
{
    /// <summary>
    /// اعتبارسنجی یکپارچه برای تاریخچه ورود
    /// </summary>
    public class LoginHistoryValidator : AbstractValidator<LoginHistoryDto>
    {
        public LoginHistoryValidator()
        {
            // اعتبارسنجی شناسه
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("شناسه الزامی است.");

            // اعتبارسنجی تاریخ و زمان ورود
            RuleFor(x => x.LoginTime)
                .NotEmpty()
                .WithMessage("تاریخ و زمان ورود الزامی است.");

            // اعتبارسنجی آدرس IP
            RuleFor(x => x.IpAddress).ApplyIpAddressRules();

            // اعتبارسنجی اطلاعات مرورگر
            RuleFor(x => x.UserAgent).ApplyDeviceInfoRules();

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

            // اعتبارسنجی دلیل ناموفق بودن
            RuleFor(x => x.FailureReason)
                .NotEmpty()
                .When(x => !x.IsSuccessful)
                .WithMessage("در صورت ناموفق بودن ورود، دلیل آن الزامی است.")
                .MaximumLength(200)
                .WithMessage("دلیل ناموفق بودن نمی‌تواند بیشتر از ۲۰۰ کاراکتر باشد.");

            // اعتبارسنجی زمان خروج
            RuleFor(x => x.LogoutTime)
                .GreaterThan(x => x.LoginTime)
                .When(x => x.LogoutTime.HasValue)
                .WithMessage("زمان خروج باید بعد از زمان ورود باشد.");

            // اعتبارسنجی مدت زمان حضور
            RuleFor(x => x.SessionDuration)
                .GreaterThan(TimeSpan.Zero)
                .When(x => x.SessionDuration.HasValue)
                .WithMessage("مدت زمان حضور باید بزرگتر از صفر باشد.");
        }
    }

    /// <summary>
    /// اعتبارسنجی برای درخواست ثبت تاریخچه ورود
    /// </summary>
    public class LoginHistoryRequestValidator : AbstractValidator<LoginHistoryRequestDto>
    {
        public LoginHistoryRequestValidator()
        {
            // اعتبارسنجی آدرس IP
            RuleFor(x => x.IpAddress).ApplyIpAddressRules();

            // اعتبارسنجی اطلاعات مرورگر
            RuleFor(x => x.UserAgent).ApplyDeviceInfoRules();

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

            // اعتبارسنجی دلیل ناموفق بودن
            RuleFor(x => x.FailureReason)
                .MaximumLength(200)
                .WithMessage("دلیل ناموفق بودن نمی‌تواند بیشتر از ۲۰۰ کاراکتر باشد.");
        }
    }
} 