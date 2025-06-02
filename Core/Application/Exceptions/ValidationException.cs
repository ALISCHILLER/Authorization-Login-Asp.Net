using System;
using System.Collections.Generic;
using FluentValidation.Results;

namespace Authorization_Login_Asp.Net.Core.Application.Exceptions
{
    /// <summary>
    /// استثنای اعتبارسنجی
    /// </summary>
    public class ValidationException : Exception
    {
        public ValidationException()
            : base("یک یا چند خطای اعتبارسنجی رخ داده است.")
        {
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(IEnumerable<ValidationFailure> failures)
            : this()
        {
            Errors = failures
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
        }

        public IDictionary<string, string[]> Errors { get; }
    }
}
