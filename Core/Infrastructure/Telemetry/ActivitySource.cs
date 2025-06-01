using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Telemetry
{
    public static class TelemetryActivitySource
    {
        private static ActivitySource _instance;

        public static ActivitySource Instance
        {
            get
            {
                if (_instance == null)
                {
                    var tracingService = ServiceLocator.GetService<ITracingService>();
                    _instance = tracingService.CreateActivitySource("Authorization.Login.Service");
                }
                return _instance;
            }
        }
    }

    public static class ServiceLocator
    {
        private static IServiceProvider _serviceProvider;

        public static void Initialize(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public static T GetService<T>()
        {
            if (_serviceProvider == null)
            {
                throw new InvalidOperationException("ServiceLocator has not been initialized.");
            }
            return _serviceProvider.GetRequiredService<T>();
        }
    }
}