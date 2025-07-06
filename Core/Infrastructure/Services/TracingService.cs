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
        private readonly ICurrentUserService _currentUserService;
        private static readonly ConcurrentDictionary<string, ActivitySource> _activitySources = new();
        private static readonly ActivitySource _defaultActivitySource;
        private static readonly ConcurrentDictionary<string, Activity> _activeOperations = new();
        private static readonly ConcurrentDictionary<string, DateTime> _operationStartTimes = new();
        private static bool _isTracingEnabled = true;
        private static string _currentCorrelationId;

        static TracingService()
        {
            _defaultActivitySource = new ActivitySource("Authorization-Login-Service");
            _activitySources.TryAdd("Authorization-Login-Service", _defaultActivitySource);
        }

        public TracingService(ILogger<TracingService> logger, IOptions<JaegerSettings> settings, ICurrentUserService currentUserService)
        {
            _logger = logger;
            _settings = settings.Value;
            _currentUserService = currentUserService;
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
            
            if (activity != null)
            {
                // Add user context
                if (_currentUserService.IsAuthenticated)
                {
                    activity.SetTag("userId", _currentUserService.UserId);
                    activity.SetTag("userName", _currentUserService.UserName);
                }

                // Add request context
                activity.SetTag("ipAddress", _currentUserService.GetIpAddress());
                activity.SetTag("userAgent", _currentUserService.GetUserAgent());
            }

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
                activity.SetStatus(ActivityStatusCode.Error);
                activity.SetTag("error.type", ex.GetType().Name);
                activity.SetTag("error.message", ex.Message);
                activity.SetTag("error.stack_trace", ex.StackTrace);

                _logger.LogError(
                    ex,
                    "Error in operation {Operation}: {ErrorMessage}",
                    name,
                    ex.Message);
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

        public async Task StartTraceAsync(string operationName, string correlationId = null)
        {
            if (string.IsNullOrEmpty(operationName))
                throw new ArgumentException("Operation name cannot be empty", nameof(operationName));

            correlationId ??= Guid.NewGuid().ToString();
            _currentCorrelationId = correlationId;

            var activity = StartActivity(operationName, ActivityKind.Internal);
            _activeOperations.TryAdd(operationName, activity);
            _operationStartTimes.TryAdd(operationName, DateTime.UtcNow);

            await Task.CompletedTask;
        }

        public async Task EndTraceAsync(string operationName, string correlationId = null)
        {
            if (string.IsNullOrEmpty(operationName))
                throw new ArgumentException("Operation name cannot be empty", nameof(operationName));

            if (_activeOperations.TryRemove(operationName, out var activity))
            {
                activity?.Stop();
                _operationStartTimes.TryRemove(operationName, out _);
            }

            await Task.CompletedTask;
        }

        public async Task AddTraceAttributeAsync(string operationName, string key, string value, string correlationId = null)
        {
            if (string.IsNullOrEmpty(operationName))
                throw new ArgumentException("Operation name cannot be empty", nameof(operationName));

            if (_activeOperations.TryGetValue(operationName, out var activity))
            {
                activity?.SetTag(key, value);
            }

            await Task.CompletedTask;
        }

        public async Task AddTraceEventAsync(string operationName, string eventName, string correlationId = null)
        {
            if (string.IsNullOrEmpty(operationName))
                throw new ArgumentException("Operation name cannot be empty", nameof(operationName));

            if (_activeOperations.TryGetValue(operationName, out var activity))
            {
                activity?.AddEvent(new ActivityEvent(eventName));
            }

            await Task.CompletedTask;
        }

        public Task<string> GetCorrelationIdAsync()
        {
            return Task.FromResult(_currentCorrelationId ?? string.Empty);
        }

        public Task SetCorrelationIdAsync(string correlationId)
        {
            _currentCorrelationId = correlationId;
            return Task.CompletedTask;
        }

        public Task<bool> IsTraceEnabledAsync()
        {
            return Task.FromResult(_isTracingEnabled);
        }

        public Task EnableTraceAsync()
        {
            _isTracingEnabled = true;
            return Task.CompletedTask;
        }

        public Task DisableTraceAsync()
        {
            _isTracingEnabled = false;
            return Task.CompletedTask;
        }

        public Task<TimeSpan> GetOperationDurationAsync(string operationName, string correlationId = null)
        {
            if (string.IsNullOrEmpty(operationName))
                throw new ArgumentException("Operation name cannot be empty", nameof(operationName));

            if (_operationStartTimes.TryGetValue(operationName, out var startTime))
            {
                var duration = DateTime.UtcNow - startTime;
                return Task.FromResult(duration);
            }

            return Task.FromResult(TimeSpan.Zero);
        }

        public Task<IEnumerable<string>> GetActiveOperationsAsync()
        {
            return Task.FromResult(_activeOperations.Keys.AsEnumerable());
        }

        public void TraceError(Exception exception, string operation)
        {
            var activity = Activity.Current;
            if (activity != null)
            {
                activity.SetStatus(ActivityStatusCode.Error);
                activity.SetTag("error.type", exception.GetType().Name);
                activity.SetTag("error.message", exception.Message);
                activity.SetTag("error.stack_trace", exception.StackTrace);

                _logger.LogError(
                    exception,
                    "Error in operation {Operation}: {ErrorMessage}",
                    operation,
                    exception.Message);
            }
        }

        public void TraceMetric(string metricName, double value, IDictionary<string, object> tags = null)
        {
            var activity = Activity.Current;
            if (activity != null)
            {
                activity.AddEvent(new ActivityEvent(
                    name: "Metric",
                    tags: new ActivityTagsCollection(new Dictionary<string, object>
                    {
                        { "metric.name", metricName },
                        { "metric.value", value },
                        { "timestamp", DateTimeOffset.UtcNow }
                    }.Concat(tags ?? new Dictionary<string, object>()))));

                _logger.LogInformation(
                    "Metric {MetricName}: {MetricValue}",
                    metricName,
                    value);
            }
        }
    }
}