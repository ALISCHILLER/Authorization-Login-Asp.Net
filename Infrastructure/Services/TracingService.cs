using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;
using System.Diagnostics;

namespace Authorization_Login_Asp.Net.Infrastructure.Services
{
    public interface ITracingService
    {
        ActivitySource CreateActivitySource(string name);
        void AddTracing(IServiceCollection services);
    }

    public class TracingService : ITracingService
    {
        private readonly ILogger<TracingService> _logger;
        private readonly TracingSettings _settings;
        private readonly ActivitySource _activitySource;

        public TracingService(
            ILogger<TracingService> logger,
            IOptions<TracingSettings> settings)
        {
            _logger = logger;
            _settings = settings.Value;
            _activitySource = new ActivitySource(_settings.ServiceName);
        }

        public ActivitySource CreateActivitySource(string name)
        {
            return new ActivitySource(name);
        }

        public void AddTracing(IServiceCollection services)
        {
            if (!_settings.EnableTracing)
            {
                _logger.LogInformation("Distributed Tracing is disabled");
                return;
            }

            services.AddOpenTelemetryTracing(builder =>
            {
                builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService(
                            serviceName: _settings.ServiceName,
                            serviceVersion: _settings.ServiceVersion,
                            serviceInstanceId: Environment.MachineName))
                    .SetSampler(new TraceIdRatioBasedSampler(_settings.SamplingRatio));

                if (_settings.EnableConsoleExporter)
                {
                    builder.AddConsoleExporter();
                }

                if (_settings.EnableJaegerExporter)
                {
                    builder.AddJaegerExporter(opts =>
                    {
                        opts.AgentHost = new Uri(_settings.JaegerEndpoint).Host;
                        opts.AgentPort = new Uri(_settings.JaegerEndpoint).Port;
                    });
                }

                builder.AddSource(_settings.ServiceName);
                builder.AddHttpClientInstrumentation();
                builder.AddAspNetCoreInstrumentation();
                builder.AddSqlClientInstrumentation();
                builder.AddRedisInstrumentation();
            });

            _logger.LogInformation("Distributed Tracing configured successfully");
        }
    }
} 