using FluentValidation;
using Authorization_Login_Asp.Net.Core.Domain.Enums;
using Authorization_Login_Asp.Net.Core.Application.DTOs;

namespace Authorization_Login_Asp.Net.Core.Application.Validators
{
    /// <summary>
    /// اعتبارسنجی برای پاسخ راه‌اندازی احراز هویت دو مرحله‌ای
    /// </summary>
    public class TwoFactorSetupResponseValidator : AbstractValidator<TwoFactorSetupResponse>
    {
        public TwoFactorSetupResponseValidator()
        {
            // اعتبارسنجی نوع احراز هویت
            RuleFor(x => x.Type)
                .NotEmpty()
                .WithMessage("نوع احراز هویت الزامی است.")
                .Must(type => Enum.TryParse(typeof(TwoFactorType), type, ignoreCase: true, out _))
                .WithMessage("نوع احراز هویت باید یکی از مقادیر معتبر باشد.");

            // اعتبارسنجی کلید مخفی
            RuleFor(x => x.SecretKey)
                .NotEmpty()
                .WithMessage("کلید مخفی الزامی است.")
                .Length(32)
                .WithMessage("کلید مخفی باید ۳۲ کاراکتر باشد.");

            // اعتبارسنجی آدرس QR کد
            RuleFor(x => x.QrCodeUrl)
                .NotEmpty()
                .WithMessage("آدرس QR کد الزامی است.")
                .MaximumLength(1000)
                .WithMessage("آدرس QR کد نمی‌تواند بیشتر از ۱۰۰۰ کاراکتر باشد.");

            // اعتبارسنجی کدهای بازیابی
            RuleFor(x => x.RecoveryCodes)
                .NotEmpty()
                .WithMessage("کدهای بازیابی الزامی هستند.")
                .Must(codes => codes.Count >= 8)
                .WithMessage("حداقل ۸ کد بازیابی باید تولید شود.");

            // اعتبارسنجی تاریخ انقضای کدهای بازیابی
            RuleFor(x => x.RecoveryCodesExpiresAt)
                .NotEmpty()
                .WithMessage("تاریخ انقضای کدهای بازیابی الزامی است.")
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("تاریخ انقضای کدهای بازیابی باید در آینده باشد.");
        }
    }
}