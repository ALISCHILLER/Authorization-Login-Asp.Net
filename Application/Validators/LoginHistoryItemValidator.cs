using FluentValidation;
using Authorization_Login_Asp.Net.Application.DTOs;

namespace Authorization_Login_Asp.Net.Application.Validators
{
    /// <summary>
    /// اعتبارسنجی برای آیتم‌های تاریخچه ورود
    /// </summary>
    public class LoginHistoryItemValidator : AbstractValidator<LoginHistoryItem>
    {
        public LoginHistoryItemValidator()
        {
            // اعتبارسنجی تاریخ و زمان ورود
            RuleFor(x => x.LoginTime)
                .NotEmpty()
                .WithMessage("تاریخ و زمان ورود الزامی است.");

            // اعتبارسنجی آدرس IP
            RuleFor(x => x.IpAddress)
                .NotEmpty()
                .WithMessage("آدرس IP الزامی است.")
                .Matches(@"^(\d{1,3}\.){3}\d{1,3}$")
                .WithMessage("فرمت آدرس IP معتبر نیست.");

            // اعتبارسنجی اطلاعات مرورگر
            RuleFor(x => x.UserAgent)
                .NotEmpty()
                .WithMessage("اطلاعات مرورگر الزامی است.")
                .MaximumLength(500)
                .WithMessage("اطلاعات مرورگر نمی‌تواند بیشتر از ۵۰۰ کاراکتر باشد.");

            // اعتبارسنجی موقعیت جغرافیایی
            RuleFor(x => x.Location)
                .MaximumLength(200)
                .WithMessage("موقعیت جغرافیایی نمی‌تواند بیشتر از ۲۰۰ کاراکتر باشد.");

            // اعتبارسنجی دلیل ناموفق بودن
            RuleFor(x => x.FailureReason)
                .NotEmpty()
                .When(x => !x.IsSuccessful)
                .WithMessage("در صورت ناموفق بودن ورود، دلیل آن الزامی است.")
                .MaximumLength(500)
                .WithMessage("دلیل ناموفق بودن نمی‌تواند بیشتر از ۵۰۰ کاراکتر باشد.");
        }
    }
} 