using FluentValidation;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Auth;
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
            // اعتبارسنجی نام کاربری
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("نام کاربری الزامی است")
                .Length(3, 50).WithMessage("نام کاربری باید بین 3 تا 50 کاراکتر باشد")
                .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("نام کاربری فقط می‌تواند شامل حروف انگلیسی، اعداد و _ باشد");

            // اعتبارسنجی ایمیل
            RuleFor(x => x.Email).ApplyEmailRules();

            // اعتبارسنجی رمز عبور
            RuleFor(x => x.Password).ApplyPasswordRules();

            // اعتبارسنجی تکرار رمز عبور
            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("تکرار رمز عبور الزامی است.")
                .Equal(x => x.Password).WithMessage("رمز عبور و تکرار آن مطابقت ندارند.");

            // اعتبارسنجی نام
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("نام الزامی است")
                .MaximumLength(50).WithMessage("نام نمی‌تواند بیشتر از 50 کاراکتر باشد");

            // اعتبارسنجی نام خانوادگی
            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("نام خانوادگی الزامی است")
                .MaximumLength(50).WithMessage("نام خانوادگی نمی‌تواند بیشتر از 50 کاراکتر باشد");

            // اعتبارسنجی شماره تلفن
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("شماره تلفن الزامی است")
                .Matches(@"^09[0-9]{9}$").WithMessage("شماره تلفن باید با 09 شروع شود و 11 رقم باشد");

            // اعتبارسنجی تاریخ تولد
            RuleFor(x => x.BirthDate)
                .NotEmpty().WithMessage("تاریخ تولد الزامی است");

            // اعتبارسنجی کد ملی
            RuleFor(x => x.NationalCode)
                .NotEmpty().WithMessage("کد ملی الزامی است")
                .Length(10).WithMessage("کد ملی باید 10 رقم باشد")
                .Matches(@"^[0-9]{10}$").WithMessage("کد ملی باید فقط شامل اعداد باشد");

            // اعتبارسنجی آدرس
            RuleFor(x => x.Address)
                .MaximumLength(500).WithMessage("آدرس نمی‌تواند بیشتر از 500 کاراکتر باشد");

            // اعتبارسنجی اطلاعات دستگاه
            RuleFor(x => x.DeviceInfo)
                .NotNull().WithMessage("اطلاعات دستگاه الزامی است");
            When(x => x.DeviceInfo != null, () => {
                RuleFor(x => x.DeviceInfo.DeviceId)
                    .NotEmpty().WithMessage("شناسه دستگاه الزامی است")
                    .MaximumLength(100).WithMessage("شناسه دستگاه نمی‌تواند بیشتر از 100 کاراکتر باشد");
                RuleFor(x => x.DeviceInfo.DeviceName)
                    .NotEmpty().WithMessage("نام دستگاه الزامی است")
                    .MaximumLength(100).WithMessage("نام دستگاه نمی‌تواند بیشتر از 100 کاراکتر باشد");
                RuleFor(x => x.DeviceInfo.DeviceType)
                    .NotEmpty().WithMessage("نوع دستگاه الزامی است")
                    .MaximumLength(50).WithMessage("نوع دستگاه نمی‌تواند بیشتر از 50 کاراکتر باشد");
                RuleFor(x => x.DeviceInfo.OperatingSystem)
                    .NotEmpty().WithMessage("سیستم عامل الزامی است")
                    .MaximumLength(100).WithMessage("سیستم عامل نمی‌تواند بیشتر از 100 کاراکتر باشد");
                RuleFor(x => x.DeviceInfo.Browser)
                    .NotEmpty().WithMessage("مرورگر الزامی است")
                    .MaximumLength(100).WithMessage("مرورگر نمی‌تواند بیشتر از 100 کاراکتر باشد");
                RuleFor(x => x.DeviceInfo.UserAgent)
                    .NotEmpty().WithMessage("User Agent الزامی است")
                    .MaximumLength(500).WithMessage("User Agent نمی‌تواند بیشتر از 500 کاراکتر باشد");
            });

            // اعتبارسنجی موقعیت مکانی (اختیاری)
            When(x => x.Location != null, () =>
            {
                RuleFor(x => x.Location.Country)
                    .MaximumLength(100).WithMessage("نام کشور نمی‌تواند بیشتر از 100 کاراکتر باشد");
                RuleFor(x => x.Location.City)
                    .MaximumLength(100).WithMessage("نام شهر نمی‌تواند بیشتر از 100 کاراکتر باشد");
            });
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
                .NotNull().WithMessage("اطلاعات دستگاه الزامی است");
            When(x => x.DeviceInfo != null, () => {
                RuleFor(x => x.DeviceInfo.DeviceId)
                    .NotEmpty().WithMessage("شناسه دستگاه الزامی است")
                    .MaximumLength(100).WithMessage("شناسه دستگاه نمی‌تواند بیشتر از 100 کاراکتر باشد");
                RuleFor(x => x.DeviceInfo.DeviceName)
                    .NotEmpty().WithMessage("نام دستگاه الزامی است")
                    .MaximumLength(100).WithMessage("نام دستگاه نمی‌تواند بیشتر از 100 کاراکتر باشد");
                RuleFor(x => x.DeviceInfo.DeviceType)
                    .NotEmpty().WithMessage("نوع دستگاه الزامی است")
                    .MaximumLength(50).WithMessage("نوع دستگاه نمی‌تواند بیشتر از 50 کاراکتر باشد");
                RuleFor(x => x.DeviceInfo.OperatingSystem)
                    .NotEmpty().WithMessage("سیستم عامل الزامی است")
                    .MaximumLength(100).WithMessage("سیستم عامل نمی‌تواند بیشتر از 100 کاراکتر باشد");
                RuleFor(x => x.DeviceInfo.Browser)
                    .NotEmpty().WithMessage("مرورگر الزامی است")
                    .MaximumLength(100).WithMessage("مرورگر نمی‌تواند بیشتر از 100 کاراکتر باشد");
                RuleFor(x => x.DeviceInfo.UserAgent)
                    .NotEmpty().WithMessage("User Agent الزامی است")
                    .MaximumLength(500).WithMessage("User Agent نمی‌تواند بیشتر از 500 کاراکتر باشد");
            });

            // اعتبارسنجی آدرس IP
            RuleFor(x => x.IpAddress).ApplyIpAddressRules();
        }
    }
} 