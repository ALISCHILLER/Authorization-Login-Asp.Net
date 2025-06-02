using FluentValidation;
using Authorization_Login_Asp.Net.Core.Application.DTOs;

namespace Authorization_Login_Asp.Net.Core.Application.Common.Validators
{
    /// <summary>
    /// اعتبارسنج برای مدل TwoFactorRequest
    /// </summary>
    public class TwoFactorRequestValidator : AbstractValidator<TwoFactorRequest>
    {
        public TwoFactorRequestValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("شناسه کاربر الزامی است");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("کد تایید الزامی است")
                .Matches(@"^[0-9]{6}$").WithMessage("کد تایید باید 6 رقم باشد");

            RuleFor(x => x.Provider)
                .NotEmpty().WithMessage("روش ارسال کد تایید الزامی است")
                .IsInEnum().WithMessage("روش ارسال کد تایید نامعتبر است");

            RuleFor(x => x.RememberDevice)
                .NotNull().WithMessage("وضعیت به خاطر سپاری دستگاه الزامی است");

            RuleFor(x => x.DeviceInfo)
                .NotEmpty().WithMessage("اطلاعات دستگاه الزامی است")
                .MaximumLength(500).WithMessage("اطلاعات دستگاه نمی‌تواند بیشتر از 500 کاراکتر باشد");

            RuleFor(x => x.IpAddress)
                .NotEmpty().WithMessage("آدرس IP الزامی است")
                .MaximumLength(45).WithMessage("آدرس IP نمی‌تواند بیشتر از 45 کاراکتر باشد")
                .Matches(@"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$|^([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}$")
                .WithMessage("فرمت آدرس IP نامعتبر است");

            RuleFor(x => x.Location)
                .MaximumLength(200).WithMessage("موقعیت مکانی نمی‌تواند بیشتر از 200 کاراکتر باشد");
        }
    }
} 