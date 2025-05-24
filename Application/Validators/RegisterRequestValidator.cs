using FluentValidation;
using Authorization_Login_Asp.Net.Application.DTOs;
using Authorization_Login_Asp.Net.Domain.Enums;

namespace Authorization_Login_Asp.Net.Application.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("ایمیل الزامی است.")
                .EmailAddress().WithMessage("فرمت ایمیل معتبر نیست.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("رمز عبور الزامی است.")
                .MinimumLength(6).WithMessage("رمز عبور باید حداقل ۶ کاراکتر باشد.")
                .Matches(@"[A-Z]").WithMessage("رمز عبور باید حداقل یک حرف بزرگ داشته باشد.")
                .Matches(@"[a-z]").WithMessage("رمز عبور باید حداقل یک حرف کوچک داشته باشد.")
                .Matches(@"\d").WithMessage("رمز عبور باید حداقل یک عدد داشته باشد.")
                .Matches(@"[!@#$%^&*(),.?""{}|<>]").WithMessage("رمز عبور باید حداقل یک نماد داشته باشد.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("تکرار رمز عبور الزامی است.")
                .Equal(x => x.Password).WithMessage("رمز عبور و تکرار آن مطابقت ندارند.");

            RuleFor(x => x.FullName)
                .MaximumLength(100).WithMessage("نام کامل نمی‌تواند بیشتر از ۱۰۰ کاراکتر باشد.");

            RuleFor(x => x.Role)
                .Must(role => Enum.TryParse(typeof(UserRole), role, ignoreCase: true, out _))
                .WithMessage("نقش باید یکی از مقادیر معتبر User، Admin یا Manager باشد.");
        }
    }
}
