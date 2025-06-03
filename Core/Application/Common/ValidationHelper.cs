using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Authorization_Login_Asp.Net.Core.Domain.Interfaces;

namespace Authorization_Login_Asp.Net.Core.Application.Common
{
    /// <summary>
    /// کلاس کمکی برای اعتبارسنجی
    /// </summary>
    public static class ValidationHelper
    {
        private static readonly Regex EmailRegex = new Regex(
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled);

        private static readonly Regex UsernameRegex = new Regex(
            @"^[a-zA-Z0-9_]{3,20}$",
            RegexOptions.Compiled);

        private static readonly Regex PasswordRegex = new Regex(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            RegexOptions.Compiled);

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
        public static void ValidateName(string name, string paramName)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("نام نمی‌تواند خالی باشد", paramName);
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

            if (!UsernameRegex.IsMatch(username))
                throw new ArgumentException("فرمت نام کاربری نامعتبر است", paramName);
        }

        /// <summary>
        /// اعتبارسنجی رمز عبور
        /// </summary>
        public static void ValidatePassword(string password, string paramName)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("رمز عبور نمی‌تواند خالی باشد", paramName);

            if (!PasswordRegex.IsMatch(password))
                throw new ArgumentException(
                    "رمز عبور باید حداقل 8 کاراکتر و شامل حروف بزرگ، حروف کوچک، اعداد و کاراکترهای خاص باشد",
                    paramName);
        }

        /// <summary>
        /// اعتبارسنجی لیست
        /// </summary>
        public static void ValidateList<T>(IEnumerable<T> list, string paramName)
        {
            if (list == null || !list.Any())
                throw new ArgumentException("لیست نمی‌تواند خالی باشد", paramName);
        }

        /// <summary>
        /// اعتبارسنجی تاریخ
        /// </summary>
        public static void ValidateDate(DateTime date, string paramName)
        {
            if (date == default)
                throw new ArgumentException("تاریخ نامعتبر است", paramName);
        }

        /// <summary>
        /// اعتبارسنجی محدوده عددی
        /// </summary>
        public static void ValidateRange(int value, int min, int max, string paramName)
        {
            if (value < min || value > max)
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