using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Authorization_Login_Asp.Net.Core.Domain.Enums;

namespace Authorization_Login_Asp.Net.Core.Domain.Exceptions
{
    /// <summary>
    /// کلاس خطای دامنه
    /// این کلاس برای خطاهای مرتبط با قوانین دامنه استفاده می‌شود
    /// </summary>
    [Serializable]
    public class DomainException : Exception
    {
        /// <summary>
        /// کد خطا
        /// </summary>
        public string? Code { get; }

        /// <summary>
        /// اطلاعات اضافی خطا
        /// </summary>
        public IDictionary<string, object>? AdditionalData { get; }

        /// <summary>
        /// زمان وقوع خطا
        /// </summary>
        public DateTime ErrorTime { get; }

        /// <summary>
        /// سازنده پیش‌فرض
        /// </summary>
        public DomainException() : base() { }

        /// <summary>
        /// سازنده با پیام خطا
        /// </summary>
        /// <param name="message">پیام خطا</param>
        public DomainException(string message) : base(message) { }

        /// <summary>
        /// سازنده با پیام خطا و خطای داخلی
        /// </summary>
        /// <param name="message">پیام خطا</param>
        /// <param name="innerException">خطای داخلی</param>
        public DomainException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// ایجاد یک نمونه جدید از استثنای دامنه
        /// </summary>
        /// <param name="message">پیام خطا</param>
        /// <param name="code">کد خطا</param>
        /// <param name="additionalData">اطلاعات اضافی خطا</param>
        public DomainException(string message, string? code, IDictionary<string, object>? additionalData = null)
            : base(message)
        {
            Code = code;
            AdditionalData = additionalData ?? new Dictionary<string, object>();
            ErrorTime = DateTime.UtcNow;
        }

        /// <summary>
        /// ایجاد یک نمونه جدید از استثنای دامنه با استثنای داخلی
        /// </summary>
        /// <param name="message">پیام خطا</param>
        /// <param name="innerException">استثنای داخلی</param>
        /// <param name="code">کد خطا</param>
        /// <param name="additionalData">اطلاعات اضافی خطا</param>
        public DomainException(string message, Exception innerException, string? code = null, IDictionary<string, object>? additionalData = null)
            : base(message, innerException)
        {
            Code = code;
            AdditionalData = additionalData ?? new Dictionary<string, object>();
            ErrorTime = DateTime.UtcNow;
        }

        /// <summary>
        /// سازنده مورد نیاز برای سریال‌سازی
        /// </summary>
        protected DomainException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Code = info.GetString(nameof(Code));
            AdditionalData = (IDictionary<string, object>?)info.GetValue(nameof(AdditionalData), typeof(IDictionary<string, object>)) ?? new Dictionary<string, object>();
            ErrorTime = info.GetDateTime(nameof(ErrorTime));
        }

        /// <summary>
        /// اضافه کردن اطلاعات به استثنا برای سریال‌سازی
        /// </summary>
        [Obsolete("This API supports obsolete formatter-based serialization.")]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Code), Code);
            info.AddValue(nameof(AdditionalData), AdditionalData);
            info.AddValue(nameof(ErrorTime), ErrorTime);
        }

        /// <summary>
        /// اضافه کردن اطلاعات اضافی به استثنا
        /// </summary>
        /// <param name="key">کلید</param>
        /// <param name="value">مقدار</param>
        public void AddData(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            AdditionalData[key] = value;
        }

        /// <summary>
        /// تبدیل استثنا به رشته
        /// </summary>
        public override string ToString()
        {
            var result = $"[{Code}] {Message}";
            
            if (AdditionalData.Count > 0)
            {
                result += "\nAdditional Data:";
                foreach (var item in AdditionalData)
                {
                    result += $"\n  {item.Key}: {item.Value}";
                }
            }

            if (InnerException != null)
            {
                result += $"\nInner Exception: {InnerException}";
            }

            return result;
        }

        /// <summary>
        /// ایجاد خطای دامنه با فرمت پیام
        /// </summary>
        /// <param name="messageFormat">قالب پیام</param>
        /// <param name="args">پارامترهای پیام</param>
        /// <returns>خطای دامنه</returns>
        public static DomainException Create(string messageFormat, params object[] args)
        {
            return new DomainException(string.Format(messageFormat, args));
        }
    }
}
