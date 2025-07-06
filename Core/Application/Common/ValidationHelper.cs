using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Authorization_Login_Asp.Net.Core.Domain.Common;

namespace Authorization_Login_Asp.Net.Core.Application.Common
{
    /// <summary>
    /// کلاس کمکی برای اعتبارسنجی
    /// </summary>
    public static class ValidationHelper
    {
        private static readonly Regex EmailRegex = new(
            @"^(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex UsernameRegex = new(
            @"^[a-zA-Z][a-zA-Z0-9_-]{2,19}$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex PasswordRegex = new(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]{8,32}$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex PhoneNumberRegex = new(
            @"^(\+98|0)?9\d{9}$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex NationalCodeRegex = new(
            @"^\d{10}$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly HashSet<string> CommonPasswords = new()
        {
            "password", "123456", "12345678", "qwerty", "admin", "test"
        };

        /// <summary>
        /// اعتبارسنجی شناسه
        /// </summary>
        public static void ValidateId(Guid id, string paramName)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("شناسه نامعتبر است", paramName);
        }

        /// <summary>
        /// اعتبارسنجی نام
        /// </summary>
        public static void ValidateName(string name, string paramName, int minLength = 2, int maxLength = 100)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("نام نمی‌تواند خالی باشد", paramName);

            if (name.Length < minLength || name.Length > maxLength)
                throw new ArgumentException($"طول نام باید بین {minLength} و {maxLength} کاراکتر باشد", paramName);
        }

        /// <summary>
        /// اعتبارسنجی موجودیت
        /// </summary>
        public static void ValidateEntity<T>(T entity, string paramName) where T : class
        {
            if (entity == null)
                throw new ArgumentNullException(paramName, "موجودیت نمی‌تواند خالی باشد");
        }

        /// <summary>
        /// اعتبارسنجی ایمیل
        /// </summary>
        public static void ValidateEmail(string email, string paramName)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("ایمیل نمی‌تواند خالی باشد", paramName);

            email = email.Trim().ToLowerInvariant();

            if (email.Length > 254)
                throw new ArgumentException("طول ایمیل نمی‌تواند بیشتر از 254 کاراکتر باشد", paramName);

            if (!EmailRegex.IsMatch(email))
                throw new ArgumentException("فرمت ایمیل نامعتبر است", paramName);
        }

        /// <summary>
        /// اعتبارسنجی نام کاربری
        /// </summary>
        public static void ValidateUsername(string username, string paramName)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("نام کاربری نمی‌تواند خالی باشد", paramName);

            username = username.Trim().ToLowerInvariant();

            if (!UsernameRegex.IsMatch(username))
                throw new ArgumentException("نام کاربری باید با حرف شروع شود و فقط شامل حروف، اعداد، خط تیره و زیرخط باشد", paramName);
        }

        /// <summary>
        /// اعتبارسنجی رمز عبور
        /// </summary>
        public static void ValidatePassword(string password, string paramName)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("رمز عبور نمی‌تواند خالی باشد", paramName);

            if (password.Length < 8 || password.Length > 32)
                throw new ArgumentException("طول رمز عبور باید بین 8 و 32 کاراکتر باشد", paramName);

            if (!PasswordRegex.IsMatch(password))
                throw new ArgumentException(
                    "رمز عبور باید شامل حروف بزرگ، حروف کوچک، اعداد و کاراکترهای خاص باشد",
                    paramName);

            if (CommonPasswords.Contains(password.ToLowerInvariant()))
                throw new ArgumentException("این رمز عبور بسیار ساده و نا‌امن است", paramName);
        }

        /// <summary>
        /// اعتبارسنجی شماره تلفن همراه
        /// </summary>
        public static void ValidatePhoneNumber(string phoneNumber, string paramName)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("شماره تلفن همراه نمی‌تواند خالی باشد", paramName);

            phoneNumber = phoneNumber.Trim();

            if (!PhoneNumberRegex.IsMatch(phoneNumber))
                throw new ArgumentException("فرمت شماره تلفن همراه نامعتبر است", paramName);
        }

        /// <summary>
        /// اعتبارسنجی کد ملی
        /// </summary>
        public static void ValidateNationalCode(string nationalCode, string paramName)
        {
            if (string.IsNullOrWhiteSpace(nationalCode))
                throw new ArgumentException("کد ملی نمی‌تواند خالی باشد", paramName);

            nationalCode = nationalCode.Trim();

            if (!NationalCodeRegex.IsMatch(nationalCode))
                throw new ArgumentException("فرمت کد ملی نامعتبر است", paramName);

            // الگوریتم اعتبارسنجی کد ملی
            var check = Convert.ToInt32(nationalCode.Substring(9, 1));
            var sum = Enumerable.Range(0, 9)
                .Select(x => Convert.ToInt32(nationalCode.Substring(x, 1)) * (10 - x))
                .Sum();
            var remainder = sum % 11;
            var calculatedCheck = remainder < 2 ? remainder : 11 - remainder;

            if (check != calculatedCheck)
                throw new ArgumentException("کد ملی نامعتبر است", paramName);
        }

        /// <summary>
        /// اعتبارسنجی لیست
        /// </summary>
        public static void ValidateList<T>(IEnumerable<T> list, string paramName, int? minCount = null, int? maxCount = null)
        {
            if (list == null)
                throw new ArgumentNullException(paramName, "لیست نمی‌تواند خالی باشد");

            var count = list.Count();

            if (minCount.HasValue && count < minCount.Value)
                throw new ArgumentException($"تعداد آیتم‌های لیست نمی‌تواند کمتر از {minCount.Value} باشد", paramName);

            if (maxCount.HasValue && count > maxCount.Value)
                throw new ArgumentException($"تعداد آیتم‌های لیست نمی‌تواند بیشتر از {maxCount.Value} باشد", paramName);
        }

        /// <summary>
        /// اعتبارسنجی تاریخ
        /// </summary>
        public static void ValidateDate(DateTime date, string paramName, DateTime? minDate = null, DateTime? maxDate = null)
        {
            if (date == default)
                throw new ArgumentException("تاریخ نامعتبر است", paramName);

            if (minDate.HasValue && date < minDate.Value)
                throw new ArgumentException($"تاریخ نمی‌تواند قبل از {minDate.Value:yyyy/MM/dd} باشد", paramName);

            if (maxDate.HasValue && date > maxDate.Value)
                throw new ArgumentException($"تاریخ نمی‌تواند بعد از {maxDate.Value:yyyy/MM/dd} باشد", paramName);
        }

        /// <summary>
        /// اعتبارسنجی محدوده عددی
        /// </summary>
        public static void ValidateRange<T>(T value, T min, T max, string paramName) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
                throw new ArgumentException($"مقدار باید بین {min} و {max} باشد", paramName);
        }

        /// <summary>
        /// اعتبارسنجی موجودیت‌های قابل حذف
        /// </summary>
        public static void ValidateDeletable<T>(T entity, string paramName) where T : class, IDeletable
        {
            ValidateEntity(entity, paramName);
            if (entity.IsDeleted)
                throw new InvalidOperationException("این موجودیت قبلاً حذف شده است");
        }

        /// <summary>
        /// اعتبارسنجی موجودیت‌های قابل ویرایش
        /// </summary>
        public static void ValidateEditable<T>(T entity, string paramName) where T : class, IAuditable
        {
            ValidateEntity(entity, paramName);
            if (entity.IsDeleted)
                throw new InvalidOperationException("این موجودیت قبلاً حذف شده است");
        }
    }
} 