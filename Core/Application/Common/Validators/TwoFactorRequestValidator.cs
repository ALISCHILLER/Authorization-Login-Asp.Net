using FluentValidation;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Auth;

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
                .IsInEnum().WithMessage("روش ارسال کد تایید نامعتبر است");

            RuleFor(x => x.DeviceInfo)
                .NotNull().WithMessage("اطلاعات دستگاه الزامی است");
            When(x => x.DeviceInfo != null, () => {
                RuleFor(x => x.DeviceInfo.DeviceId)
                    .NotEmpty().WithMessage("شناسه دستگاه الزامی است")
                    .MaximumLength(100).WithMessage("شناسه دستگاه نمی‌تواند بیشتر از 100 کاراکتر باشد");
                RuleFor(x => x.DeviceInfo.DeviceName)
                    .NotEmpty().WithMessage("نام دستگاه الزامی است")
                    .MaximumLength(100).WithMessage("نام دستگاه نمی‌تواند بیشتر از 100 کاراکتر باشد");
                RuleFor(x => x.DeviceInfo.DeviceType)
                    .NotEmpty().WithMessage("نوع دستگاه الزامی است")
                    .MaximumLength(50).WithMessage("نوع دستگاه نمی‌تواند بیشتر از 50 کاراکتر باشد");
                RuleFor(x => x.DeviceInfo.OperatingSystem)
                    .NotEmpty().WithMessage("سیستم عامل الزامی است")
                    .MaximumLength(100).WithMessage("سیستم عامل نمی‌تواند بیشتر از 100 کاراکتر باشد");
                RuleFor(x => x.DeviceInfo.Browser)
                    .NotEmpty().WithMessage("مرورگر الزامی است")
                    .MaximumLength(100).WithMessage("مرورگر نمی‌تواند بیشتر از 100 کاراکتر باشد");
                RuleFor(x => x.DeviceInfo.UserAgent)
                    .NotEmpty().WithMessage("User Agent الزامی است")
                    .MaximumLength(500).WithMessage("User Agent نمی‌تواند بیشتر از 500 کاراکتر باشد");
            });

            When(x => x.Location != null, () =>
            {
                RuleFor(x => x.Location.Country)
                    .MaximumLength(100).WithMessage("نام کشور نمی‌تواند بیشتر از 100 کاراکتر باشد");
                RuleFor(x => x.Location.City)
                    .MaximumLength(100).WithMessage("نام شهر نمی‌تواند بیشتر از 100 کاراکتر باشد");
            });
        }
    }
} 