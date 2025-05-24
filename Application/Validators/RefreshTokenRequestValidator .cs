using FluentValidation;
using Authorization_Login_Asp.Net.Application.DTOs;

namespace Authorization_Login_Asp.Net.Application.Validators
{
    /// <summary>
    /// اعتبارسنجی برای درخواست تمدید توکن
    /// </summary>
    public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
    {
        public RefreshTokenRequestValidator()
        {
            RuleFor(x => x.AccessToken)
                .NotEmpty().WithMessage("توکن دسترسی الزامی است.");

            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("توکن رفرش الزامی است.");
        }
    }
}
