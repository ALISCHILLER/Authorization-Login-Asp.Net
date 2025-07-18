using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Infrastructure.Logging;
using Authorization_Login_Asp.Net.Core.Infrastructure.Repositories;
using Authorization_Login_Asp.Net.Core.Infrastructure.Security;
using Authorization_Login_Asp.Net.Core.Infrastructure.Services;
using Authorization_Login_Asp.Net.Core.Infrastructure.Services.Common; // Added for DateTimeService
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
            // IUserService is now registered in ApplicationExtensions pointing to Infrastructure.Services.UserService
            // services.AddScoped<IUserService, AuthenticationService>();
            // ILoginHistoryService is now registered in ApplicationExtensions pointing to Infrastructure.Services.LoginHistoryService
            // services.AddScoped<Authorization_Login_Asp.Net.Core.Application.Interfaces.Services.ILoginHistoryService, Infrastructure.Services.LoginHistoryService>();

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

            // Services moved from Program.cs or to be centralized here
            services.AddScoped<ICurrentUserService, CurrentUserService>(); // CurrentUserService is in Infrastructure.Services
            services.AddScoped<IAuditService, AuditService>();             // AuditService is in Infrastructure.Services
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();   // JwtTokenGenerator is in Infrastructure.Security
            services.AddScoped<IPasswordHasher, PasswordHasher>();         // PasswordHasher is in Infrastructure.Security
            // ILoginHistoryRepository is already registered above.
            services.AddScoped<ITwoFactorService, TwoFactorService>();     // TwoFactorService is in Infrastructure.Security (moved from Program.cs)

            // Register DateTimeService
            services.AddSingleton<IDateTimeService, Common.DateTimeService>(); // DateTimeService is in Infrastructure.Services.Common

            return services;
        }
    }
}
