using FluentValidation;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Auth;
using Authorization_Login_Asp.Net.Core.Application.Features.Authentication.Commands.Login;
using Authorization_Login_Asp.Net.Core.Application.Validators.Common;

namespace Authorization_Login_Asp.Net.Core.Application.Validators.Auth
{
    /// <summary>
    /// اعتبارسنجی یکپارچه برای عملیات ورود
    /// </summary>
    public class LoginValidator : AbstractValidator<LoginRequest>
    {
        public LoginValidator()
        {
            // اعتبارسنجی نام کاربری یا ایمیل
            RuleFor(x => x.UsernameOrEmail)
                .NotEmpty().WithMessage("نام کاربری یا ایمیل الزامی است")
                .MaximumLength(100).WithMessage("نام کاربری یا ایمیل نمی‌تواند بیشتر از 100 کاراکتر باشد")
                .Matches(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$|^[a-zA-Z0-9._-]{3,}$")
                .WithMessage("فرمت نام کاربری یا ایمیل نامعتبر است");

            // اعتبارسنجی رمز عبور
            RuleFor(x => x.Password).ApplyPasswordRules();

            // اعتبارسنجی اطلاعات دستگاه
            RuleFor(x => x.DeviceInfo)
                .NotNull().WithMessage("اطلاعات دستگاه الزامی است")
                .SetValidator(new DeviceInfoValidator());

            // اعتبارسنجی آدرس IP
            RuleFor(x => x.IpAddress).ApplyIpAddressRules();

            // اعتبارسنجی اطلاعات موقعیت مکانی
            When(x => x.Location != null, () =>
            {
                RuleFor(x => x.Location.Country)
                    .MaximumLength(100).WithMessage("نام کشور نمی‌تواند بیشتر از 100 کاراکتر باشد");

                RuleFor(x => x.Location.City)
                    .MaximumLength(100).WithMessage("نام شهر نمی‌تواند بیشتر از 100 کاراکتر باشد");

                RuleFor(x => x.Location.Latitude)
                    .InclusiveBetween(-90, 90).WithMessage("عرض جغرافیایی باید بین -90 و 90 باشد")
                    .When(x => x.Location.Latitude.HasValue);

                RuleFor(x => x.Location.Longitude)
                    .InclusiveBetween(-180, 180).WithMessage("طول جغرافیایی باید بین -180 و 180 باشد")
                    .When(x => x.Location.Longitude.HasValue);
            });

            // اعتبارسنجی کد احراز هویت دو مرحله‌ای
            When(x => x.TwoFactorCode != null, () =>
            {
                RuleFor(x => x.TwoFactorCode)
                    .Length(6).WithMessage("کد احراز هویت دو مرحله‌ای باید 6 رقمی باشد")
                    .Matches("^[0-9]+$").WithMessage("کد احراز هویت دو مرحله‌ای باید فقط شامل اعداد باشد");
            });
        }
    }

    /// <summary>
    /// اعتبارسنجی برای دستور ورود کاربر
    /// </summary>
    public class LoginCommandValidator : AbstractValidator<LoginUserCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Email).ApplyEmailRules();
            RuleFor(x => x.Password).ApplyPasswordRules();
            RuleFor(x => x.IpAddress).ApplyIpAddressRules();
            RuleFor(x => x.DeviceInfo).ApplyDeviceInfoRules();
        }
    }

    /// <summary>
    /// اعتبارسنجی برای اطلاعات دستگاه
    /// </summary>
    public class DeviceInfoValidator : AbstractValidator<DeviceInfoDto>
    {
        public DeviceInfoValidator()
        {
            RuleFor(x => x.DeviceId)
                .NotEmpty().WithMessage("شناسه دستگاه الزامی است");

            RuleFor(x => x.DeviceName)
                .NotEmpty().WithMessage("نام دستگاه الزامی است")
                .MaximumLength(100).WithMessage("نام دستگاه نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.DeviceType)
                .NotEmpty().WithMessage("نوع دستگاه الزامی است")
                .MaximumLength(50).WithMessage("نوع دستگاه نمی‌تواند بیشتر از 50 کاراکتر باشد");

            RuleFor(x => x.OperatingSystem)
                .NotEmpty().WithMessage("سیستم عامل الزامی است")
                .MaximumLength(50).WithMessage("سیستم عامل نمی‌تواند بیشتر از 50 کاراکتر باشد");

            RuleFor(x => x.Browser)
                .NotEmpty().WithMessage("مرورگر الزامی است")
                .MaximumLength(50).WithMessage("مرورگر نمی‌تواند بیشتر از 50 کاراکتر باشد");

            RuleFor(x => x.UserAgent)
                .NotEmpty().WithMessage("User Agent الزامی است")
                .MaximumLength(500).WithMessage("User Agent نمی‌تواند بیشتر از 500 کاراکتر باشد");
        }
    }
} 