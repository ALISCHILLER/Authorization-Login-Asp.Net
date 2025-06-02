using System;
using System.Collections.Generic;
using System.Linq;

namespace Authorization_Login_Asp.Net.Core.Domain.Common
{
    /// <summary>
    /// کلاس نتیجه برای مدیریت نتایج عملیات
    /// </summary>
    public class Result
    {
        /// <summary>
        /// آیا عملیات موفق بوده است؟
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// پیام نتیجه
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// لیست خطاها
        /// </summary>
        public IReadOnlyList<string> Errors { get; }

        /// <summary>
        /// سازنده پیش‌فرض
        /// </summary>
        /// <param name="isSuccess">نتیجه عملیات</param>
        /// <param name="message">پیام نتیجه</param>
        /// <param name="errors">لیست خطاها</param>
        protected Result(bool isSuccess, string message = null, IReadOnlyList<string> errors = null)
        {
            IsSuccess = isSuccess;
            Message = message;
            Errors = errors ?? Array.Empty<string>();
        }

        /// <summary>
        /// ایجاد یک نتیجه موفق
        /// </summary>
        /// <param name="message">پیام نتیجه</param>
        /// <returns>نتیجه موفق</returns>
        public static Result Success(string message = null)
        {
            return new Result(true, message);
        }

        /// <summary>
        /// ایجاد یک نتیجه ناموفق با یک خطا
        /// </summary>
        /// <param name="error">پیام خطا</param>
        /// <returns>نتیجه ناموفق</returns>
        /// <exception cref="ArgumentNullException">خطا نمی‌تواند خالی باشد</exception>
        public static Result Failure(string error)
        {
            if (string.IsNullOrWhiteSpace(error))
                throw new ArgumentNullException(nameof(error));

            return new Result(false, null, new[] { error });
        }

        /// <summary>
        /// ایجاد یک نتیجه ناموفق با چند خطا
        /// </summary>
        /// <param name="errors">لیست خطاها</param>
        /// <returns>نتیجه ناموفق</returns>
        /// <exception cref="ArgumentNullException">لیست خطاها نمی‌تواند خالی باشد</exception>
        /// <exception cref="ArgumentException">لیست خطاها نمی‌تواند شامل خطای خالی باشد</exception>
        public static Result Failure(IEnumerable<string> errors)
        {
            if (errors == null)
                throw new ArgumentNullException(nameof(errors));

            var errorList = errors.ToList();
            if (errorList.Count == 0)
                throw new ArgumentException("List of errors cannot be empty", nameof(errors));

            if (errorList.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException("List of errors cannot contain empty or whitespace errors", nameof(errors));

            return new Result(false, null, errorList);
        }

        /// <summary>
        /// تبدیل ضمنی از رشته به نتیجه موفق
        /// </summary>
        /// <param name="message">پیام نتیجه</param>
        public static implicit operator Result(string message) => Success(message);

        /// <summary>
        /// تبدیل ضمنی از خطا به نتیجه ناموفق
        /// </summary>
        /// <param name="error">پیام خطا</param>
        public static implicit operator Result(Exception error) => Failure(error.Message);
    }

    /// <summary>
    /// کلاس نتیجه برای عملیات‌هایی که یک مقدار برمی‌گردانند
    /// </summary>
    /// <typeparam name="T">نوع داده برگشتی</typeparam>
    public class Result<T> : Result
    {
        /// <summary>
        /// داده برگشتی
        /// </summary>
        public T Data { get; }

        /// <summary>
        /// سازنده پیش‌فرض
        /// </summary>
        /// <param name="isSuccess">نتیجه عملیات</param>
        /// <param name="data">داده برگشتی</param>
        /// <param name="message">پیام نتیجه</param>
        /// <param name="errors">لیست خطاها</param>
        protected Result(bool isSuccess, T data, string message = null, IReadOnlyList<string> errors = null)
            : base(isSuccess, message, errors)
        {
            Data = data;
        }

        /// <summary>
        /// ایجاد یک نتیجه موفق با داده
        /// </summary>
        /// <param name="data">داده برگشتی</param>
        /// <param name="message">پیام نتیجه</param>
        /// <returns>نتیجه موفق</returns>
        /// <exception cref="ArgumentNullException">داده نمی‌تواند خالی باشد</exception>
        public static Result<T> Success(T data, string message = null)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            return new Result<T>(true, data, message);
        }

        /// <summary>
        /// ایجاد یک نتیجه ناموفق با یک خطا
        /// </summary>
        /// <param name="error">پیام خطا</param>
        /// <returns>نتیجه ناموفق</returns>
        /// <exception cref="ArgumentNullException">خطا نمی‌تواند خالی باشد</exception>
        public static new Result<T> Failure(string error)
        {
            if (string.IsNullOrWhiteSpace(error))
                throw new ArgumentNullException(nameof(error));

            return new Result<T>(false, default, null, new[] { error });
        }

        /// <summary>
        /// ایجاد یک نتیجه ناموفق با چند خطا
        /// </summary>
        /// <param name="errors">لیست خطاها</param>
        /// <returns>نتیجه ناموفق</returns>
        /// <exception cref="ArgumentNullException">لیست خطاها نمی‌تواند خالی باشد</exception>
        /// <exception cref="ArgumentException">لیست خطاها نمی‌تواند شامل خطای خالی باشد</exception>
        public static new Result<T> Failure(IEnumerable<string> errors)
        {
            if (errors == null)
                throw new ArgumentNullException(nameof(errors));

            var errorList = errors.ToList();
            if (errorList.Count == 0)
                throw new ArgumentException("List of errors cannot be empty", nameof(errors));

            if (errorList.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException("List of errors cannot contain empty or whitespace errors", nameof(errors));

            return new Result<T>(false, default, null, errorList);
        }

        /// <summary>
        /// تبدیل ضمنی از داده به نتیجه موفق
        /// </summary>
        /// <param name="data">داده برگشتی</param>
        public static implicit operator Result<T>(T data) => Success(data);

        /// <summary>
        /// تبدیل ضمنی از خطا به نتیجه ناموفق
        /// </summary>
        /// <param name="error">پیام خطا</param>
        public static implicit operator Result<T>(Exception error) => Failure(error.Message);

        /// <summary>
        /// تبدیل ضمنی از نتیجه به نتیجه با داده
        /// </summary>
        /// <param name="result">نتیجه</param>
        public static implicit operator Result<T>(Result result) => 
            result.IsSuccess 
                ? throw new InvalidOperationException("Cannot convert successful Result to Result<T> without data")
                : Failure(result.Errors);
    }
}