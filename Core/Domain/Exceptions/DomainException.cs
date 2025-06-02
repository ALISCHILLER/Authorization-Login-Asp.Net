using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Authorization_Login_Asp.Net.Core.Domain.Exceptions
{
    /// <summary>
    /// کلاس پایه برای استثناهای دامنه
    /// این کلاس برای خطاهای مربوط به منطق کسب و کار و قوانین دامنه استفاده می‌شود
    /// </summary>
    [Serializable]
    public class DomainException : Exception
    {
        /// <summary>
        /// کد خطا
        /// </summary>
        public string ErrorCode { get; }

        /// <summary>
        /// اطلاعات اضافی خطا
        /// </summary>
        public IDictionary<string, object> AdditionalData { get; }

        /// <summary>
        /// زمان وقوع خطا
        /// </summary>
        public DateTime ErrorTime { get; }

        /// <summary>
        /// ایجاد یک نمونه جدید از استثنای دامنه
        /// </summary>
        /// <param name="message">پیام خطا</param>
        /// <param name="errorCode">کد خطا</param>
        /// <param name="additionalData">اطلاعات اضافی خطا</param>
        public DomainException(string message, string errorCode = DomainErrorCodes.General.InvalidOperation, IDictionary<string, object> additionalData = null)
            : base(message)
        {
            ErrorCode = errorCode;
            AdditionalData = additionalData ?? new Dictionary<string, object>();
            ErrorTime = DateTime.UtcNow;
        }

        /// <summary>
        /// ایجاد یک نمونه جدید از استثنای دامنه با استثنای داخلی
        /// </summary>
        /// <param name="message">پیام خطا</param>
        /// <param name="innerException">استثنای داخلی</param>
        /// <param name="errorCode">کد خطا</param>
        /// <param name="additionalData">اطلاعات اضافی خطا</param>
        public DomainException(string message, Exception innerException, string errorCode = DomainErrorCodes.General.InvalidOperation, IDictionary<string, object> additionalData = null)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            AdditionalData = additionalData ?? new Dictionary<string, object>();
            ErrorTime = DateTime.UtcNow;
        }

        /// <summary>
        /// سازنده مورد نیاز برای سریال‌سازی
        /// </summary>
        protected DomainException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ErrorCode = info.GetString(nameof(ErrorCode)) ?? DomainErrorCodes.General.InvalidOperation;
            AdditionalData = (IDictionary<string, object>)info.GetValue(nameof(AdditionalData), typeof(IDictionary<string, object>)) ?? new Dictionary<string, object>();
            ErrorTime = info.GetDateTime(nameof(ErrorTime));
        }

        /// <summary>
        /// اضافه کردن اطلاعات به استثنا برای سریال‌سازی
        /// </summary>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue(nameof(ErrorCode), ErrorCode);
            info.AddValue(nameof(AdditionalData), AdditionalData);
            info.AddValue(nameof(ErrorTime), ErrorTime);

            base.GetObjectData(info, context);
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
            var result = $"[{ErrorCode}] {Message}";
            
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
    }
}
