using FluentValidation;
using Authorization_Login_Asp.Net.Core.Application.DTOs;

namespace Authorization_Login_Asp.Net.Core.Application.Common.Validators
{
    /// <summary>
    /// اعتبارسنج برای مدل ChangePasswordRequest
    /// </summary>
    public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordRequestValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("رمز عبور فعلی الزامی است")
                .MaximumLength(100).WithMessage("رمز عبور فعلی نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("رمز عبور جدید الزامی است")
                .MinimumLength(8).WithMessage("رمز عبور جدید باید حداقل 8 کاراکتر باشد")
                .MaximumLength(100).WithMessage("رمز عبور جدید نمی‌تواند بیشتر از 100 کاراکتر باشد")
                .Matches(@"[A-Z]").WithMessage("رمز عبور جدید باید حداقل یک حرف بزرگ داشته باشد")
                .Matches(@"[a-z]").WithMessage("رمز عبور جدید باید حداقل یک حرف کوچک داشته باشد")
                .Matches(@"[0-9]").WithMessage("رمز عبور جدید باید حداقل یک عدد داشته باشد")
                .Matches(@"[^a-zA-Z0-9]").WithMessage("رمز عبور جدید باید حداقل یک کاراکتر خاص داشته باشد")
                .NotEqual(x => x.CurrentPassword).WithMessage("رمز عبور جدید نمی‌تواند با رمز عبور فعلی یکسان باشد");

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty().WithMessage("تکرار رمز عبور جدید الزامی است")
                .Equal(x => x.NewPassword).WithMessage("رمز عبور جدید و تکرار آن باید یکسان باشند");

            RuleFor(x => x.RequirePasswordChange)
                .NotNull().WithMessage("وضعیت اجباری بودن تغییر رمز عبور الزامی است");
        }
    }
} 