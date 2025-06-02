using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Authorization_Login_Asp.Net.Core.Domain.Common;

namespace Authorization_Login_Asp.Net.Core.Domain.ValueObjects
{
    /// <summary>
    /// Value Object برای شماره تلفن
    /// این کلاس شامل اعتبارسنجی و مدیریت شماره تلفن است
    /// </summary>
    public class PhoneNumber : ValueObject
    {
        [Required]
        [MaxLength(15)]
        public string Value { get; private set; }

        protected PhoneNumber() { }

        public static PhoneNumber Create(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("شماره تلفن نمی‌تواند خالی باشد", nameof(phoneNumber));

            phoneNumber = NormalizePhoneNumber(phoneNumber);

            if (!IsValidPhoneNumber(phoneNumber))
                throw new ArgumentException("فرمت شماره تلفن نامعتبر است", nameof(phoneNumber));

            return new PhoneNumber { Value = phoneNumber };
        }

        private static string NormalizePhoneNumber(string phoneNumber)
        {
            // حذف همه کاراکترهای غیر عددی
            var digits = Regex.Replace(phoneNumber, @"[^\d]", "");
            
            // اگر شماره با 0 شروع شده باشد، آن را حذف می‌کنیم
            if (digits.StartsWith("0"))
                digits = digits[1..];

            // اگر شماره با 98 شروع شده باشد، آن را حذف می‌کنیم
            if (digits.StartsWith("98"))
                digits = digits[2..];

            // اضافه کردن پیشوند +98
            return $"+98{digits}";
        }

        private static bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            try
            {
                // بررسی فرمت شماره تلفن ایران
                var pattern = @"^\+98[1-9]\d{9}$";
                return Regex.IsMatch(phoneNumber, pattern);
            }
            catch
            {
                return false;
            }
        }

        public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber?.Value;
        public static explicit operator PhoneNumber(string phoneNumber) => Create(phoneNumber);

        public override string ToString() => Value;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
} 