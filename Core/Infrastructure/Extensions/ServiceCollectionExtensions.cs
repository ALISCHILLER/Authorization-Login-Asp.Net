using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Infrastructure.Logging;
using Authorization_Login_Asp.Net.Core.Infrastructure.Repositories;
using Authorization_Login_Asp.Net.Core.Infrastructure.Security;
using Authorization_Login_Asp.Net.Core.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Extensions
{
    /// <summary>
    /// کلاس توسعه‌دهنده برای ثبت سرویس‌های زیرساختی
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// افزودن سرویس‌های زیرساختی به کانتینر DI
        /// </summary>
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // ثبت ریپوزیتوری‌ها
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<ILoginHistoryRepository, LoginHistoryRepository>();

            // ثبت سرویس‌های امنیتی
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IUserService, AuthenticationService>();
            services.AddScoped<ILoginHistoryService, AuthenticationService>();

            // ثبت سرویس‌های ارتباطی
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ISmsService, SmsService>();
            services.AddScoped<INotificationService, NotificationService>();

            // ثبت سرویس‌های پشتیبانی
            services.AddScoped<ILoggingService, LoggingAndErrorHandlingService>();
            services.AddScoped<IErrorHandlingService, LoggingAndErrorHandlingService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<ITracingService, TracingService>();
            services.AddScoped<IMetricsService, MetricsService>();

            // ثبت سرویس‌های کش
            services.AddMemoryCache();
            services.AddScoped<ICacheService, CacheService>();

            // ثبت سرویس‌های مانیتورینگ
            services.AddHealthChecks()
                .AddDbContextCheck<ApplicationDbContext>()
                .AddCheck<ExternalServiceHealthCheck>("External Services")
                .AddCheck<DatabaseHealthCheck>("Database");

            return services;
        }
    }
}
