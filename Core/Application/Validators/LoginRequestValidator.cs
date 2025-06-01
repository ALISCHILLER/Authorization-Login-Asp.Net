using FluentValidation;
using Authorization_Login_Asp.Net.Core.Application.DTOs;

namespace Authorization_Login_Asp.Net.Core.Application.Validators
{
    /// <summary>
    /// اعتبارسنجی برای درخواست ورود به سیستم (Login)
    /// </summary>
    public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("نام کاربری نمی‌تواند خالی باشد")
                .MinimumLength(3).WithMessage("نام کاربری باید حداقل 3 کاراکتر باشد")
                .MaximumLength(50).WithMessage("نام کاربری نمی‌تواند بیشتر از 50 کاراکتر باشد");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("رمز عبور نمی‌تواند خالی باشد")
                .MinimumLength(8).WithMessage("رمز عبور باید حداقل 8 کاراکتر باشد")
                .MaximumLength(100).WithMessage("رمز عبور نمی‌تواند بیشتر از 100 کاراکتر باشد")
                .Matches("[A-Z]").WithMessage("رمز عبور باید حداقل یک حرف بزرگ داشته باشد")
                .Matches("[a-z]").WithMessage("رمز عبور باید حداقل یک حرف کوچک داشته باشد")
                .Matches("[0-9]").WithMessage("رمز عبور باید حداقل یک عدد داشته باشد")
                .Matches("[^a-zA-Z0-9]").WithMessage("رمز عبور باید حداقل یک کاراکتر خاص داشته باشد");

            RuleFor(x => x.DeviceId)
                .NotEmpty().WithMessage("شناسه دستگاه نمی‌تواند خالی باشد");

            RuleFor(x => x.DeviceName)
                .NotEmpty().WithMessage("نام دستگاه نمی‌تواند خالی باشد")
                .MaximumLength(100).WithMessage("نام دستگاه نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.DeviceType)
                .NotEmpty().WithMessage("نوع دستگاه نمی‌تواند خالی باشد")
                .MaximumLength(50).WithMessage("نوع دستگاه نمی‌تواند بیشتر از 50 کاراکتر باشد");

            RuleFor(x => x.IpAddress)
                .NotEmpty().WithMessage("آدرس IP نمی‌تواند خالی باشد")
                .MaximumLength(50).WithMessage("آدرس IP نمی‌تواند بیشتر از 50 کاراکتر باشد");

            RuleFor(x => x.UserAgent)
                .NotEmpty().WithMessage("User-Agent نمی‌تواند خالی باشد")
                .MaximumLength(500).WithMessage("User-Agent نمی‌تواند بیشتر از 500 کاراکتر باشد");

            RuleFor(x => x.RememberMe)
                .NotNull().WithMessage("مشخص کنید که آیا می‌خواهید در سیستم بمانید");

            When(x => x.TwoFactorCode != null, () =>
            {
                RuleFor(x => x.TwoFactorCode)
                    .Length(6).WithMessage("کد احراز هویت دو مرحله‌ای باید 6 رقمی باشد")
                    .Matches("^[0-9]+$").WithMessage("کد احراز هویت دو مرحله‌ای باید فقط شامل اعداد باشد");
            });
        }
    }
}
