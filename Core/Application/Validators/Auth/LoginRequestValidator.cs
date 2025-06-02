using FluentValidation;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Auth;

namespace Authorization_Login_Asp.Net.Core.Application.Validators.Auth
{
    /// <summary>
    /// اعتبارسنجی درخواست ورود
    /// </summary>
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.UsernameOrEmail)
                .NotEmpty().WithMessage("نام کاربری یا ایمیل الزامی است")
                .MaximumLength(100).WithMessage("نام کاربری یا ایمیل نمی‌تواند بیشتر از 100 کاراکتر باشد")
                .Matches(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$|^[a-zA-Z0-9._-]{3,}$")
                .WithMessage("فرمت نام کاربری یا ایمیل نامعتبر است");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("رمز عبور الزامی است")
                .MinimumLength(6).WithMessage("رمز عبور باید حداقل 6 کاراکتر باشد")
                .MaximumLength(100).WithMessage("رمز عبور نمی‌تواند بیشتر از 100 کاراکتر باشد")
                .Matches("[A-Z]").WithMessage("رمز عبور باید شامل حداقل یک حرف بزرگ باشد")
                .Matches("[a-z]").WithMessage("رمز عبور باید شامل حداقل یک حرف کوچک باشد")
                .Matches("[0-9]").WithMessage("رمز عبور باید شامل حداقل یک عدد باشد")
                .Matches("[^a-zA-Z0-9]").WithMessage("رمز عبور باید شامل حداقل یک کاراکتر خاص باشد");

            RuleFor(x => x.DeviceInfo)
                .NotNull().WithMessage("اطلاعات دستگاه الزامی است")
                .SetValidator(new DeviceInfoValidator());

            RuleFor(x => x.IpAddress)
                .NotEmpty().WithMessage("آدرس IP الزامی است")
                .Matches(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$").WithMessage("فرمت آدرس IP نامعتبر است");

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
        }
    }

    /// <summary>
    /// اعتبارسنجی اطلاعات دستگاه
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