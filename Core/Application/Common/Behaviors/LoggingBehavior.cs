using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;

namespace Authorization_Login_Asp.Net.Core.Application.Common.Behaviors;

/// <summary>
/// رفتار لاگینگ برای درخواست‌های MediatR
/// این رفتار اطلاعات مربوط به درخواست و پاسخ را در لاگ ثبت می‌کند
/// </summary>
/// <typeparam name="TRequest">نوع درخواست</typeparam>
/// <typeparam name="TResponse">نوع پاسخ</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        MaxDepth = 5,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// سازنده رفتار لاگینگ
    /// </summary>
    /// <param name="logger">لاگر</param>
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// پردازش درخواست و ثبت لاگ
    /// </summary>
    /// <param name="request">درخواست</param>
    /// <param name="next">مرحله بعدی در خط پردازش</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>پاسخ درخواست</returns>
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
        var requestProperties = GetRequestProperties(request);

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["RequestType"] = requestType,
            ["RequestProperties"] = requestProperties
        });

        var timer = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            _logger.LogInformation(
                "شروع پردازش درخواست {RequestType} با شناسه {CorrelationId}\nپارامترها: {RequestProperties}",
                requestType,
                correlationId,
                JsonSerializer.Serialize(requestProperties, _jsonOptions));

            var response = await next();
            timer.Stop();

            var responseProperties = GetResponseProperties(response);

            _logger.LogInformation(
                "پردازش درخواست {RequestType} با شناسه {CorrelationId} با موفقیت انجام شد\n" +
                "زمان پردازش: {ElapsedMilliseconds} میلی‌ثانیه\n" +
                "پاسخ: {ResponseProperties}",
                requestType,
                correlationId,
                timer.ElapsedMilliseconds,
                JsonSerializer.Serialize(responseProperties, _jsonOptions));

            return response;
        }
        catch (Exception ex)
        {
            timer.Stop();

            _logger.LogError(
                ex,
                "خطا در پردازش درخواست {RequestType} با شناسه {CorrelationId}\n" +
                "زمان پردازش: {ElapsedMilliseconds} میلی‌ثانیه\n" +
                "پارامترها: {RequestProperties}\n" +
                "پیام خطا: {ErrorMessage}\n" +
                "محل خطا: {StackTrace}",
                requestType,
                correlationId,
                timer.ElapsedMilliseconds,
                JsonSerializer.Serialize(requestProperties, _jsonOptions),
                ex.Message,
                ex.StackTrace);

            throw;
        }
    }

    /// <summary>
    /// استخراج پراپرتی‌های درخواست برای لاگ
    /// </summary>
    private static Dictionary<string, object> GetRequestProperties(TRequest request)
    {
        try
        {
            return request.GetType()
                .GetProperties()
                .Where(p => p.CanRead && IsSimpleType(p.PropertyType))
                .ToDictionary(
                    p => p.Name,
                    p => p.GetValue(request));
        }
        catch (Exception)
        {
            return new Dictionary<string, object>
            {
                ["warning"] = "خطا در استخراج پراپرتی‌های درخواست"
            };
        }
    }

    /// <summary>
    /// استخراج پراپرتی‌های پاسخ برای لاگ
    /// </summary>
    private static Dictionary<string, object> GetResponseProperties(TResponse response)
    {
        if (response == null)
        {
            return new Dictionary<string, object>
            {
                ["warning"] = "پاسخ خالی است"
            };
        }

        try
        {
            return response.GetType()
                .GetProperties()
                .Where(p => p.CanRead && IsSimpleType(p.PropertyType))
                .ToDictionary(
                    p => p.Name,
                    p => p.GetValue(response));
        }
        catch (Exception)
        {
            return new Dictionary<string, object>
            {
                ["warning"] = "خطا در استخراج پراپرتی‌های پاسخ"
            };
        }
    }

    /// <summary>
    /// بررسی نوع ساده برای لاگ کردن
    /// </summary>
    private static bool IsSimpleType(Type type)
    {
        return type.IsPrimitive
            || type.IsEnum
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(TimeSpan)
            || type == typeof(Guid)
            || IsNullableSimpleType(type);
    }

    /// <summary>
    /// بررسی نوع nullable ساده
    /// </summary>
    private static bool IsNullableSimpleType(Type type)
    {
        return type.IsGenericType
            && type.GetGenericTypeDefinition() == typeof(Nullable<>)
            && IsSimpleType(type.GetGenericArguments()[0]);
    }
} 