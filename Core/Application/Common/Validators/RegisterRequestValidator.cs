using FluentValidation;
using Authorization_Login_Asp.Net.Core.Application.DTOs;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Auth;
using System;
using System.Linq;

namespace Authorization_Login_Asp.Net.Core.Application.Common.Validators
{
    /// <summary>
    /// اعتبارسنج برای مدل RegisterRequest
    /// </summary>
    public class RegisterRequestValidator : BaseValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            ValidateUsername(RuleFor(x => x.Username));
            ValidateEmail(RuleFor(x => x.Email));
            ValidatePassword(RuleFor(x => x.Password));

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("تکرار رمز عبور الزامی است")
                .Equal(x => x.Password).WithMessage("رمز عبور و تکرار آن باید یکسان باشند");

            ValidatePersianName(RuleFor(x => x.FirstName), "نام");
            ValidatePersianName(RuleFor(x => x.LastName), "نام خانوادگی");
            ValidatePhoneNumber(RuleFor(x => x.PhoneNumber));

            RuleFor(x => x.BirthDate)
                .NotEmpty().WithMessage("تاریخ تولد الزامی است")
                .LessThan(DateTime.Now).WithMessage("تاریخ تولد نمی‌تواند در آینده باشد")
                .GreaterThan(DateTime.Now.AddYears(-100)).WithMessage("تاریخ تولد نامعتبر است");

            RuleFor(x => x.NationalCode)
                .NotEmpty().WithMessage("کد ملی الزامی است")
                .Matches(@"^[0-9]{10}$").WithMessage("فرمت کد ملی نامعتبر است")
                .Must(BeValidNationalCode).WithMessage("کد ملی نامعتبر است");

            RuleFor(x => x.Address)
                .MaximumLength(500).WithMessage("آدرس نمی‌تواند بیشتر از 500 کاراکتر باشد");

            RuleFor(x => x.ProfileImage)
                .MaximumLength(1000).WithMessage("آدرس تصویر پروفایل نمی‌تواند بیشتر از 1000 کاراکتر باشد")
                .Matches(@"^data:image\/(jpeg|png|gif);base64,").When(x => !string.IsNullOrEmpty(x.ProfileImage))
                .WithMessage("فرمت تصویر پروفایل نامعتبر است");
        }

        private bool BeValidNationalCode(string nationalCode)
        {
            if (string.IsNullOrEmpty(nationalCode) || nationalCode.Length != 10)
                return false;

            var check = Convert.ToInt32(nationalCode.Substring(9, 1));
            var sum = Enumerable.Range(0, 9)
                .Select(x => Convert.ToInt32(nationalCode.Substring(x, 1)) * (10 - x))
                .Sum() % 11;

            return sum < 2 ? check == sum : check + sum == 11;
        }
    }
} 