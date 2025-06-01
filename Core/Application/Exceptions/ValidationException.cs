using System;
using System.Collections.Generic;

namespace Authorization_Login_Asp.Net.Core.Application.Exceptions
{
    /// <summary>
    /// استثنا برای خطاهای اعتبارسنجی که ممکن است شامل چندین پیام خطا باشد
    /// </summary>
    public class ValidationException : Exception
    {
        /// <summary>
        /// لیست پیام‌های خطای اعتبارسنجی
        /// </summary>
        public List<string> Errors { get; }

        /// <summary>
        /// سازنده پیش‌فرض
        /// </summary>
        public ValidationException()
            : base("خطاهای اعتبارسنجی رخ داده است.")
        {
            Errors = new List<string>();
        }

        /// <summary>
        /// سازنده با لیست خطاها
        /// </summary>
        /// <param name="errors">لیست پیام‌های خطا</param>
        public ValidationException(List<string> errors)
            : base("خطاهای اعتبارسنجی رخ داده است.")
        {
            Errors = errors;
        }

        /// <summary>
        /// سازنده با پیام خطا
        /// </summary>
        /// <param name="message">پیام خطا</param>
        public ValidationException(string message)
            : base(message)
        {
            Errors = new List<string> { message };
        }
    }
}
