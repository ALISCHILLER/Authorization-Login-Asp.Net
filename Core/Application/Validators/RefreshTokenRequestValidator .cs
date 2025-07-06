using FluentValidation;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Auth;

namespace Authorization_Login_Asp.Net.Core.Application.Validators
{
    /// <summary>
    /// اعتبارسنجی برای درخواست تمدید توکن
    /// </summary>
    public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
    {
        public RefreshTokenRequestValidator()
        {
            RuleFor(x => x.ExpiredAccessToken)
                .NotEmpty().WithMessage("توکن دسترسی الزامی است.");

            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("توکن رفرش الزامی است.");
        }
    }
}
