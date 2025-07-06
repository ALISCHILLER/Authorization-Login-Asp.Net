using FluentValidation;
using Authorization_Login_Asp.Net.Core.Application.DTOs;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Auth;

namespace Authorization_Login_Asp.Net.Core.Application.Common.Validators
{
    /// <summary>
    /// اعتبارسنج برای مدل ChangePasswordRequest
    /// </summary>
    public class ChangePasswordRequestValidator : BaseValidator<ChangePasswordRequest>
    {
        public ChangePasswordRequestValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("رمز عبور فعلی الزامی است")
                .MaximumLength(100).WithMessage("رمز عبور فعلی نمی‌تواند بیشتر از 100 کاراکتر باشد");

            ValidatePassword(RuleFor(x => x.NewPassword))
                .NotEqual(x => x.CurrentPassword).WithMessage("رمز عبور جدید نمی‌تواند با رمز عبور فعلی یکسان باشد");

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty().WithMessage("تکرار رمز عبور جدید الزامی است")
                .Equal(x => x.NewPassword).WithMessage("رمز عبور جدید و تکرار آن باید یکسان باشند");

            RuleFor(x => x.RequirePasswordChange)
                .NotNull().WithMessage("وضعیت اجباری بودن تغییر رمز عبور الزامی است");
        }
    }
} 