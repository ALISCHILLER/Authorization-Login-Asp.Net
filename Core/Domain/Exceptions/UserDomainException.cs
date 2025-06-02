using System;
using System.Collections.Generic;

namespace Authorization_Login_Asp.Net.Core.Domain.Exceptions
{
    /// <summary>
    /// استثنای مربوط به خطاهای دامنه کاربر
    /// این کلاس برای خطاهای مربوط به عملیات کاربر مانند ثبت‌نام، ورود، تغییر رمز عبور و غیره استفاده می‌شود
    /// </summary>
    public class UserDomainException : DomainException
    {
        /// <summary>
        /// شناسه کاربر (در صورت وجود)
        /// </summary>
        public Guid? UserId { get; }

        /// <summary>
        /// نام کاربری (در صورت وجود)
        /// </summary>
        public string Username { get; }

        /// <summary>
        /// ایجاد یک نمونه جدید از استثنای دامنه کاربر
        /// </summary>
        /// <param name="message">پیام خطا</param>
        /// <param name="errorCode">کد خطا</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="username">نام کاربری</param>
        /// <param name="additionalData">اطلاعات اضافی</param>
        public UserDomainException(
            string message,
            string errorCode = DomainErrorCodes.User.UserNotFound,
            Guid? userId = null,
            string username = null,
            IDictionary<string, object> additionalData = null)
            : base(message, errorCode, additionalData)
        {
            UserId = userId;
            Username = username;

            if (userId.HasValue)
                AddData("UserId", userId.Value);
            if (!string.IsNullOrEmpty(username))
                AddData("Username", username);
        }

        /// <summary>
        /// ایجاد یک نمونه جدید از استثنای دامنه کاربر با استثنای داخلی
        /// </summary>
        /// <param name="message">پیام خطا</param>
        /// <param name="innerException">استثنای داخلی</param>
        /// <param name="errorCode">کد خطا</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="username">نام کاربری</param>
        /// <param name="additionalData">اطلاعات اضافی</param>
        public UserDomainException(
            string message,
            Exception innerException,
            string errorCode = DomainErrorCodes.User.UserNotFound,
            Guid? userId = null,
            string username = null,
            IDictionary<string, object> additionalData = null)
            : base(message, innerException, errorCode, additionalData)
        {
            UserId = userId;
            Username = username;

            if (userId.HasValue)
                AddData("UserId", userId.Value);
            if (!string.IsNullOrEmpty(username))
                AddData("Username", username);
        }

        /// <summary>
        /// تبدیل استثنا به رشته
        /// </summary>
        public override string ToString()
        {
            var result = base.ToString();

            if (UserId.HasValue || !string.IsNullOrEmpty(Username))
            {
                result = result.Insert(result.IndexOf('\n'), 
                    $" (User: {Username ?? "Unknown"} {UserId?.ToString() ?? ""})");
            }

            return result;
        }
    }
} 