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
using OpenTelemetry.Instrumentation.EntityFrameworkCore;
using System.Collections.Concurrent;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Infrastructure.Configurations;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Services
{
    /// <summary>
    /// پیاده‌سازی سرویس ردیابی توزیع شده
    /// </summary>
    public class TracingService : ITracingService
    {
        private readonly ILogger<TracingService> _logger;
        private readonly JaegerSettings _settings;
        private static readonly ConcurrentDictionary<string, ActivitySource> _activitySources = new();
        private static readonly ActivitySource _defaultActivitySource;

        static TracingService()
        {
            _defaultActivitySource = new ActivitySource("Authorization-Login-Service");
            _activitySources.TryAdd("Authorization-Login-Service", _defaultActivitySource);
        }

        public TracingService(ILogger<TracingService> logger, IOptions<JaegerSettings> settings)
        {
            _logger = logger;
            _settings = settings.Value;
        }

        /// <inheritdoc/>
        public ActivitySource CreateActivitySource(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("نام منبع فعالیت نمی‌تواند خالی باشد", nameof(name));

            return _activitySources.GetOrAdd(name, n => new ActivitySource(n));
        }

        /// <inheritdoc/>
        public void AddTracing(IServiceCollection services)
        {
            try
            {
                services.AddOpenTelemetry()
                    .WithTracing(builder =>
                    {
                        builder
                            .AddSource(_defaultActivitySource.Name)
                            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Authorization-Login-Service"))
                            .AddAspNetCoreInstrumentation()
                            .AddHttpClientInstrumentation()
                            .AddEntityFrameworkCoreInstrumentation(options =>
                            {
                                options.SetDbStatementForText = true;
                            })
                            .AddRuntimeInstrumentation()
                            .AddJaegerExporter(opts =>
                            {
                                opts.AgentHost = _settings.Host;
                                opts.AgentPort = _settings.Port;
                                opts.Endpoint = new Uri($"{_settings.Protocol}://{_settings.Host}:{_settings.Port}");
                            });
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configuring OpenTelemetry");
                throw;
            }
        }

        /// <inheritdoc/>
        public Activity StartActivity(string name, ActivityKind kind = ActivityKind.Internal, ActivityContext? parentContext = null, IEnumerable<KeyValuePair<string, object>>? attributes = null)
        {
            var activity = _defaultActivitySource.StartActivity(name, kind, parentContext ?? default, attributes);
            return activity ?? new Activity(name);
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
        public async Task ExecuteInActivityAsync(string name, Func<Task> operation, ActivityKind kind = ActivityKind.Internal, IEnumerable<KeyValuePair<string, object>>? attributes = null)
        {
            using var activity = StartActivity(name, kind, attributes: attributes);
            try
            {
                await operation();
                activity.SetStatus(ActivityStatusCode.Ok);
            }
            catch (Exception ex)
            {
                activity.SetStatus(ActivityStatusCode.Error, ex.Message);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<T> ExecuteInActivityAsync<T>(string name, Func<Task<T>> operation, ActivityKind kind = ActivityKind.Internal, IEnumerable<KeyValuePair<string, object>>? attributes = null)
        {
            using var activity = StartActivity(name, kind, attributes: attributes);
            try
            {
                var result = await operation();
                activity.SetStatus(ActivityStatusCode.Ok);
                return result;
            }
            catch (Exception ex)
            {
                activity.SetStatus(ActivityStatusCode.Error, ex.Message);
                throw;
            }
        }
    }
}