using System;
using System.Collections.Generic;

namespace Authorization_Login_Asp.Net.Domain.Common
{
    /// <summary>
    /// کلاس نتیجه برای مدیریت نتایج عملیات
    /// </summary>
    public class Result
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; }

        public static Result Success(string message = null)
        {
            return new Result
            {
                IsSuccess = true,
                Message = message
            };
        }

        public static Result Failure(string error)
        {
            return new Result
            {
                IsSuccess = false,
                Errors = new List<string> { error }
            };
        }

        public static Result Failure(List<string> errors)
        {
            return new Result
            {
                IsSuccess = false,
                Errors = errors
            };
        }
    }

    /// <summary>
    /// کلاس نتیجه برای عملیات‌هایی که یک مقدار برمی‌گردانند
    /// </summary>
    public class Result<T> : Result
    {
        public T Data { get; set; }

        public static Result<T> Success(T data, string message = null)
        {
            return new Result<T>
            {
                IsSuccess = true,
                Message = message,
                Data = data
            };
        }

        public static new Result<T> Failure(string error)
        {
            return new Result<T>
            {
                IsSuccess = false,
                Errors = new List<string> { error }
            };
        }

        public static new Result<T> Failure(List<string> errors)
        {
            return new Result<T>
            {
                IsSuccess = false,
                Errors = errors
            };
        }
    }
} 