using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
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
            _validators = validators ?? throw new ArgumentNullException(nameof(validators));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var requestType = typeof(TRequest).Name;
            var correlationId = Guid.NewGuid().ToString();

            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["RequestType"] = requestType
            });

            try
            {
                if (!_validators.Any())
                {
                    _logger.LogDebug(
                        "هیچ اعتبارسنجی برای درخواست {RequestType} با شناسه {CorrelationId} تعریف نشده است",
                        requestType,
                        correlationId);
                    return await next();
                }

                _logger.LogInformation(
                    "شروع اعتبارسنجی درخواست {RequestType} با شناسه {CorrelationId}",
                    requestType,
                    correlationId);

                var context = new ValidationContext<TRequest>(request);
                var validationResults = new List<ValidationResult>();

                foreach (var validator in _validators)
                {
                    try
                    {
                        var result = await validator.ValidateAsync(context, cancellationToken);
                        validationResults.Add(result);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "خطا در اجرای اعتبارسنج {ValidatorType} برای درخواست {RequestType} با شناسه {CorrelationId}",
                            validator.GetType().Name,
                            requestType,
                            correlationId);
                        throw;
                    }
                }

                var failures = validationResults
                    .SelectMany(r => r.Errors)
                    .Where(f => f != null)
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.ErrorMessage).Distinct().ToArray());

                if (failures.Any())
                {
                    _logger.LogWarning(
                        "اعتبارسنجی درخواست {RequestType} با شناسه {CorrelationId} ناموفق بود. تعداد خطاها: {ErrorCount}",
                        requestType,
                        correlationId,
                        failures.Count);

                    foreach (var failure in failures)
                    {
                        _logger.LogWarning(
                            "خطای اعتبارسنجی در فیلد {PropertyName}: {ErrorMessages}",
                            failure.Key,
                            string.Join(", ", failure.Value));
                    }

                    throw new ValidationException(validationResults.SelectMany(r => r.Errors));
                }

                _logger.LogInformation(
                    "اعتبارسنجی درخواست {RequestType} با شناسه {CorrelationId} با موفقیت انجام شد",
                    requestType,
                    correlationId);

                return await next();
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "خطای غیرمنتظره در اعتبارسنجی درخواست {RequestType} با شناسه {CorrelationId}",
                    requestType,
                    correlationId);
                throw;
            }
        }
    }
} 