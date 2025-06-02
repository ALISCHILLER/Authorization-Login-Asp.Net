using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ValidationException = Authorization_Login_Asp.Net.Core.Application.Exceptions.ValidationException;

namespace Authorization_Login_Asp.Net.Core.Application.Common.Behaviors
{
    /// <summary>
    /// رفتار اعتبارسنجی برای درخواست‌های MediatR
    /// این کلاس قبل از اجرای درخواست، اعتبارسنجی‌های تعریف شده را انجام می‌دهد
    /// </summary>
    /// <typeparam name="TRequest">نوع درخواست</typeparam>
    /// <typeparam name="TResponse">نوع پاسخ</typeparam>
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

        /// <summary>
        /// سازنده رفتار اعتبارسنجی
        /// </summary>
        /// <param name="validators">اعتبارسنج‌های درخواست</param>
        /// <param name="logger">لاگر</param>
        public ValidationBehavior(
            IEnumerable<IValidator<TRequest>> validators,
            ILogger<ValidationBehavior<TRequest, TResponse>> logger)
        {
            _validators = validators;
            _logger = logger;
        }

        /// <summary>
        /// پردازش درخواست و اعتبارسنجی آن
        /// </summary>
        /// <param name="request">درخواست</param>
        /// <param name="next">مرحله بعدی در خط پردازش</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>پاسخ درخواست</returns>
        /// <exception cref="ValidationException">در صورت نامعتبر بودن درخواست</exception>
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (!_validators.Any())
            {
                return await next();
            }

            _logger.LogInformation("شروع اعتبارسنجی درخواست {RequestType}", typeof(TRequest).Name);

            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Any())
            {
                _logger.LogWarning(
                    "اعتبارسنجی درخواست {RequestType} ناموفق بود. خطاها: {ValidationErrors}",
                    typeof(TRequest).Name,
                    string.Join(", ", failures.Select(f => f.ErrorMessage)));

                throw new ValidationException(failures);
            }

            _logger.LogInformation("اعتبارسنجی درخواست {RequestType} با موفقیت انجام شد", typeof(TRequest).Name);
            return await next();
        }
    }
} 