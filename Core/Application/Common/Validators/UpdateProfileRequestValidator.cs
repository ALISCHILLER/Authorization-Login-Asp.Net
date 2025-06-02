using FluentValidation;
using Authorization_Login_Asp.Net.Core.Application.DTOs;

namespace Authorization_Login_Asp.Net.Core.Application.Common.Validators
{
    /// <summary>
    /// اعتبارسنج برای مدل UpdateProfileRequest
    /// </summary>
    public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
    {
        public UpdateProfileRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("نام الزامی است")
                .MaximumLength(50).WithMessage("نام نمی‌تواند بیشتر از 50 کاراکتر باشد")
                .Matches(@"^[\u0600-\u06FF\s]+$").WithMessage("نام باید به فارسی وارد شود");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("نام خانوادگی الزامی است")
                .MaximumLength(50).WithMessage("نام خانوادگی نمی‌تواند بیشتر از 50 کاراکتر باشد")
                .Matches(@"^[\u0600-\u06FF\s]+$").WithMessage("نام خانوادگی باید به فارسی وارد شود");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("ایمیل الزامی است")
                .EmailAddress().WithMessage("فرمت ایمیل نامعتبر است")
                .MaximumLength(100).WithMessage("ایمیل نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("شماره تلفن الزامی است")
                .Matches(@"^09[0-9]{9}$").WithMessage("فرمت شماره تلفن نامعتبر است")
                .MaximumLength(11).WithMessage("شماره تلفن نمی‌تواند بیشتر از 11 کاراکتر باشد");

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