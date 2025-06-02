using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

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

    /// <summary>
    /// سازنده رفتار لاگینگ
    /// </summary>
    /// <param name="logger">لاگر</param>
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
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
        var requestName = typeof(TRequest).Name;
        var requestGuid = Guid.NewGuid().ToString();

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["RequestName"] = requestName,
            ["RequestGuid"] = requestGuid,
            ["RequestType"] = typeof(TRequest).FullName,
            ["ResponseType"] = typeof(TResponse).FullName
        });

        _logger.LogInformation(
            "شروع پردازش درخواست {RequestName} با شناسه {RequestGuid}",
            requestName,
            requestGuid);

        var timer = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var response = await next();

            timer.Stop();

            _logger.LogInformation(
                "پردازش درخواست {RequestName} با شناسه {RequestGuid} با موفقیت انجام شد. زمان پردازش: {ElapsedMilliseconds} میلی‌ثانیه",
                requestName,
                requestGuid,
                timer.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            timer.Stop();

            _logger.LogError(
                ex,
                "خطا در پردازش درخواست {RequestName} با شناسه {RequestGuid}. زمان پردازش: {ElapsedMilliseconds} میلی‌ثانیه",
                requestName,
                requestGuid,
                timer.ElapsedMilliseconds);

            throw;
        }
    }
} 