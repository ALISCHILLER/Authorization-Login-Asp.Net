using FluentValidation;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Auth;
using Authorization_Login_Asp.Net.Core.Application.Features.Authentication.Commands.Register;
using Authorization_Login_Asp.Net.Core.Application.Validators.Common;
using Authorization_Login_Asp.Net.Core.Domain.Enums;

namespace Authorization_Login_Asp.Net.Core.Application.Validators.Auth
{
    /// <summary>
    /// اعتبارسنجی یکپارچه برای عملیات ثبت‌نام
    /// </summary>
    public class RegisterValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterValidator()
        {
            // اعتبارسنجی ایمیل
            RuleFor(x => x.Email).ApplyEmailRules();

            // اعتبارسنجی رمز عبور
            RuleFor(x => x.Password).ApplyPasswordRules();

            // اعتبارسنجی تکرار رمز عبور
            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("تکرار رمز عبور الزامی است.")
                .Equal(x => x.Password).WithMessage("رمز عبور و تکرار آن مطابقت ندارند.");

            // اعتبارسنجی نام کامل
            RuleFor(x => x.FullName).ApplyFullNameRules();

            // اعتبارسنجی نقش کاربر
            RuleFor(x => x.Role)
                .Must(role => Enum.TryParse(typeof(UserRole), role, ignoreCase: true, out _))
                .WithMessage("نقش باید یکی از مقادیر معتبر User، Admin یا Manager باشد.");

            // اعتبارسنجی اطلاعات دستگاه
            RuleFor(x => x.DeviceInfo)
                .NotNull().WithMessage("اطلاعات دستگاه الزامی است")
                .SetValidator(new DeviceInfoValidator());

            // اعتبارسنجی آدرس IP
            RuleFor(x => x.IpAddress).ApplyIpAddressRules();
        }
    }

    /// <summary>
    /// اعتبارسنجی برای دستور ثبت‌نام کاربر
    /// </summary>
    public class RegisterCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterCommandValidator()
        {
            // اعتبارسنجی نام کاربری
            RuleFor(x => x.Username).ApplyUsernameRules();

            // اعتبارسنجی ایمیل
            RuleFor(x => x.Email).ApplyEmailRules();

            // اعتبارسنجی رمز عبور
            RuleFor(x => x.Password).ApplyPasswordRules();

            // اعتبارسنجی نام کامل
            RuleFor(x => x.FullName).ApplyFullNameRules();

            // اعتبارسنجی اطلاعات دستگاه
            RuleFor(x => x.DeviceInfo)
                .NotNull().WithMessage("اطلاعات دستگاه الزامی است")
                .SetValidator(new DeviceInfoValidator());

            // اعتبارسنجی آدرس IP
            RuleFor(x => x.IpAddress).ApplyIpAddressRules();
        }
    }
} 