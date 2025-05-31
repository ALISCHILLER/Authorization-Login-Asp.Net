using System;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

namespace Authorization_Login_Asp.Net.Infrastructure.Extensions
{
    public static class OpenTelemetryExtensions
    {
        public static IServiceCollection AddOpenTelemetryTracing(this IServiceCollection services, Action<TracerProviderBuilder> configure)
        {
            services.AddOpenTelemetry()
                .WithTracing(builder =>
                {
                    configure(builder);
                });

            return services;
        }
    }
} 