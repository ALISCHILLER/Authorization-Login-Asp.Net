using Authorization_Login_Asp.Net.Infrastructure.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using OpenTelemetry.Extensions.Hosting;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Instrumentation.Http;



namespace Authorization_Login_Asp.Net.Infrastructure.Services
{
    /// <summary>
    /// سرویس ردیابی توزیع شده
    /// این سرویس برای مدیریت و پیکربندی ردیابی درخواست‌ها و عملیات سیستم استفاده می‌شود
    /// </summary>
    public interface ITracingService
    {
        /// <summary>
        /// ایجاد یک منبع فعالیت جدید
        /// </summary>
        /// <param name="name">نام منبع فعالیت</param>
        /// <returns>منبع فعالیت ایجاد شده</returns>
        ActivitySource CreateActivitySource(string name);

        /// <summary>
        /// اضافه کردن سرویس‌های ردیابی به کانتینر DI
        /// </summary>
        /// <param name="services">کالکشن سرویس‌ها</param>
        void AddTracing(IServiceCollection services);

        /// <summary>
        /// شروع یک فعالیت جدید
        /// </summary>
        /// <param name="name">نام فعالیت</param>
        /// <param name="kind">نوع فعالیت</param>
        /// <param name="parentContext">زمینه فعالیت والد (اختیاری)</param>
        /// <param name="attributes">ویژگی‌های فعالیت</param>
        /// <returns>فعالیت ایجاد شده</returns>
        Activity StartActivity(string name, ActivityKind kind = ActivityKind.Internal, ActivityContext? parentContext = null, IEnumerable<KeyValuePair<string, object>>? attributes = null);

        /// <summary>
        /// ثبت یک رویداد در فعالیت فعلی
        /// </summary>
        /// <param name="name">نام رویداد</param>
        /// <param name="attributes">ویژگی‌های رویداد</param>
        void AddEvent(string name, params (string key, object value)[] attributes);

        /// <summary>
        /// اجرای یک عملیات در یک فعالیت جدید
        /// </summary>
        /// <typeparam name="T">نوع نتیجه عملیات</typeparam>
        /// <param name="name">نام فعالیت</param>
        /// <param name="operation">عملیات مورد نظر</param>
        /// <param name="kind">نوع فعالیت</param>
        /// <param name="attributes">ویژگی‌های فعالیت</param>
        /// <returns>نتیجه عملیات</returns>
        Task<T> ExecuteInActivityAsync<T>(string name, Func<Task<T>> operation, ActivityKind kind = ActivityKind.Internal, IEnumerable<KeyValuePair<string, object>>? attributes = null);

        /// <summary>
        /// اجرای یک عملیات در یک فعالیت جدید
        /// </summary>
        /// <param name="name">نام فعالیت</param>
        /// <param name="operation">عملیات مورد نظر</param>
        /// <param name="kind">نوع فعالیت</param>
        /// <param name="attributes">ویژگی‌های فعالیت</param>
        /// <returns>عملیات</returns>
        Task ExecuteInActivityAsync(string name, Func<Task> operation, ActivityKind kind = ActivityKind.Internal, IEnumerable<KeyValuePair<string, object>>? attributes = null);
    }

    /// <summary>
    /// پیاده‌سازی سرویس ردیابی توزیع شده
    /// </summary>
    public class TracingService : ITracingService
    {
        private readonly ILogger<TracingService> _logger;
        private readonly TracingSettings _settings;
        private readonly ActivitySource _activitySource;

        public TracingService(
            ILogger<TracingService> logger,
            IOptions<TracingSettings> settings)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _activitySource = new ActivitySource(_settings.ServiceName);
        }

        /// <inheritdoc/>
        public ActivitySource CreateActivitySource(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("نام منبع فعالیت نمی‌تواند خالی باشد", nameof(name));

            return new ActivitySource(name);
        }

        /// <inheritdoc/>
        public void AddTracing(IServiceCollection services)
        {
            if (!_settings.EnableTracing)
            {
                _logger.LogInformation("ردیابی توزیع شده غیرفعال است");
                return;
            }

            try
            {
             AddOpenTelemetryTracing(builder =>
                {
                    // تنظیم منبع سرویس
                    builder.SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService(
                            serviceName: _settings.ServiceName,
                            serviceVersion: _settings.ServiceVersion,
                            serviceInstanceId: Environment.MachineName)
                        .AddAttributes(new Dictionary<string, object>
                        {
                            ["environment"] = _settings.JaegerSettings.Environment,
                            ["deployment.region"] = Environment.GetEnvironmentVariable("DEPLOYMENT_REGION") ?? "local",
                            ["host.name"] = Environment.MachineName,
                            ["os.type"] = Environment.OSVersion.Platform.ToString(),
                            ["os.version"] = Environment.OSVersion.VersionString
                        }));

                    // تنظیم نمونه‌برداری
                    builder.SetSampler(new TraceIdRatioBasedSampler(_settings.SamplingRatio));

                    // اضافه کردن خروجی کنسول
                    if (_settings.EnableConsoleExporter)
                    {
                        builder.AddConsoleExporter();
                        _logger.LogInformation("خروجی کنسول برای ردیابی فعال شد");
                    }

                    // اضافه کردن خروجی Jaeger
                    if (_settings.EnableJaegerExporter)
                    {
                        var jaegerUri = new Uri(_settings.JaegerEndpoint);
                        builder.AddJaegerExporter(opts =>
                        {
                            opts.AgentHost = jaegerUri.Host;
                            opts.AgentPort = jaegerUri.Port;
                            opts.ExportProcessorType = ExportProcessorType.Batch;
                            opts.BatchExportProcessorOptions = new BatchExportProcessorOptions<Activity>
                            {
                                MaxQueueSize = 2048,
                                ScheduledDelayMilliseconds = 5000,
                                ExporterTimeoutMilliseconds = _settings.JaegerSettings.ExportTimeout,
                                MaxExportBatchSize = 512
                            };
                        });
                        _logger.LogInformation("خروجی Jaeger برای ردیابی فعال شد - آدرس: {JaegerEndpoint}", _settings.JaegerEndpoint);
                    }

                    // اضافه کردن منبع فعالیت
                    builder.AddSource(_settings.ServiceName);

                    // اضافه کردن ابزارهای ردیابی
                    builder.AddHttpClientInstrumentation(opts =>
                    {
                        opts.RecordException = true;
                        opts.EnrichWithHttpRequestMessage = (activity, request) =>
                        {
                            activity.SetTag("http.request.headers", string.Join(", ", request.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}")));
                            activity.SetTag("http.request.method", request.Method.ToString());
                            activity.SetTag("http.request.url", request.RequestUri?.ToString());
                        };
                        opts.EnrichWithHttpResponseMessage = (activity, response) =>
                        {
                            activity.SetTag("http.response.status_code", (int)response.StatusCode);
                            activity.SetTag("http.response.headers", string.Join(", ", response.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}")));
                        };
                    });

                    builder.AddAspNetCoreInstrumentation(opts =>
                    {
                        opts.RecordException = true;
                        opts.EnrichWithHttpRequest = (activity, request) =>
                        {
                            activity.SetTag("http.request.headers", string.Join(", ", request.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}")));
                            activity.SetTag("http.request.method", request.Method);
                            activity.SetTag("http.request.url", $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}");
                            activity.SetTag("http.request.client_ip", request.HttpContext.Connection.RemoteIpAddress?.ToString());
                        };
                        opts.EnrichWithHttpResponse = (activity, response) =>
                        {
                            activity.SetTag("http.response.status_code", response.StatusCode);
                            activity.SetTag("http.response.headers", string.Join(", ", response.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}")));
                        };
                    });

                    builder.AddSqlClientInstrumentation(opts =>
                    {
                        opts.RecordException = true;
                        opts.SetDbStatementForText = true;
                        opts.EnableConnectionLevelAttributes = true;
                    });

                    builder.AddRedisInstrumentation(opts =>
                    {
                        opts.RecordException = true;
                        opts.SetDbStatementForText = true;
                    });
                });

                _logger.LogInformation("سرویس ردیابی توزیع شده با موفقیت پیکربندی شد");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در پیکربندی سرویس ردیابی توزیع شده");
                throw;
            }
        }

        /// <inheritdoc/>
        public Activity StartActivity(string name, ActivityKind kind = ActivityKind.Internal, ActivityContext? parentContext = null, IEnumerable<KeyValuePair<string, object>>? attributes = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("نام فعالیت نمی‌تواند خالی باشد", nameof(name));

            var activity = _activitySource.StartActivity(name, kind, parentContext ?? default, attributes);
            
            if (activity == null)
            {
                _logger.LogWarning("فعالیت {ActivityName} ایجاد نشد - احتمالاً نمونه‌برداری غیرفعال است", name);
                return Activity.Current ?? new Activity(name);
            }

            return activity;
        }

        /// <inheritdoc/>
        public void AddEvent(string name, params (string key, object value)[] attributes)
        {
            var activity = Activity.Current;
            if (activity == null)
            {
                _logger.LogWarning("هیچ فعالیت فعالی برای ثبت رویداد وجود ندارد");
                return;
            }

            var eventAttributes = new ActivityTagsCollection();
            foreach (var (key, value) in attributes)
            {
                eventAttributes.Add(key, value);
            }

            activity.AddEvent(new ActivityEvent(name, DateTimeOffset.UtcNow, eventAttributes));
        }

        /// <inheritdoc/>
        public async Task<T> ExecuteInActivityAsync<T>(string name, Func<Task<T>> operation, ActivityKind kind = ActivityKind.Internal, IEnumerable<KeyValuePair<string, object>>? attributes = null)
        {
            using var activity = StartActivity(name, kind, attributes: attributes);
            try
            {
                var result = await operation();
                activity?.SetStatus(ActivityStatusCode.Ok);
                return result;
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.RecordException(ex);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task ExecuteInActivityAsync(string name, Func<Task> operation, ActivityKind kind = ActivityKind.Internal, IEnumerable<KeyValuePair<string, object>>? attributes = null)
        {
            using var activity = StartActivity(name, kind, attributes: attributes);
            try
            {
                await operation();
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.RecordException(ex);
                throw;
            }
        }
    }
} 