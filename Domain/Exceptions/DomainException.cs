using System;

namespace Authorization_Login_Asp.Net.Domain.Exceptions
{
    /// <summary>
    /// استثنای عمومی مربوط به خطاهای دامنه (بیزنس)
    /// برای مدیریت خطاهای منطقی و قواعد دامنه استفاده می‌شود.
    /// </summary>
    public class DomainException : Exception
    {
        /// <summary>
        /// سازنده بدون پارامتر
        /// </summary>
        public DomainException()
        {
        }

        /// <summary>
        /// سازنده با پیام خطا
        /// </summary>
        /// <param name="message">پیام خطا</param>
        public DomainException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// سازنده با پیام خطا و استثنای داخلی (Inner Exception)
        /// </summary>
        /// <param name="message">پیام خطا</param>
        /// <param name="innerException">استثنای داخلی</param>
        public DomainException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
