using FluentValidation;
using Authorization_Login_Asp.Net.Core.Application.DTOs;

namespace Authorization_Login_Asp.Net.Core.Application.Common.Validators
{
    /// <summary>
    /// اعتبارسنج برای مدل LoginRequest
    /// </summary>
    public class LoginRequestValidator : BaseValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.UsernameOrEmail)
                .NotEmpty().WithMessage("نام کاربری یا ایمیل الزامی است")
                .MaximumLength(100).WithMessage("نام کاربری یا ایمیل نمی‌تواند بیشتر از 100 کاراکتر باشد")
                .Matches(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$|^[a-zA-Z0-9_]{4,20}$")
                .WithMessage("نام کاربری یا ایمیل نامعتبر است");

            ValidatePassword(RuleFor(x => x.Password));

            RuleFor(x => x.DeviceInfo)
                .MaximumLength(500).WithMessage("اطلاعات دستگاه نمی‌تواند بیشتر از 500 کاراکتر باشد");

            ValidateIpAddress(RuleFor(x => x.IpAddress));

            RuleFor(x => x.Location)
                .MaximumLength(200).WithMessage("موقعیت مکانی نمی‌تواند بیشتر از 200 کاراکتر باشد");
        }
    }
} 