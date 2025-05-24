using FluentValidation;
using Authorization_Login_Asp.Net.Application.DTOs;

namespace Authorization_Login_Asp.Net.Application.Validators
{
    /// <summary>
    /// اعتبارسنجی برای درخواست ورود به سیستم (Login)
    /// </summary>
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            // اعتبارسنجی ایمیل
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("ایمیل الزامی است.")
                .EmailAddress().WithMessage("فرمت ایمیل معتبر نیست.");

            // اعتبارسنجی رمز عبور
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("رمز عبور الزامی است.")
                .MinimumLength(6).WithMessage("رمز عبور باید حداقل ۶ کاراکتر باشد.");
        }
    }
}
