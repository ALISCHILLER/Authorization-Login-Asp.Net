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
            // Register repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            // Register application services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();

            // Register security services
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ITwoFactorAuthenticator, TwoFactorAuthenticator>();
            services.AddSingleton<RateLimiter>();

            // Register communication services
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ISmsService, SmsService>();

            // Register logging services
            services.AddScoped<ILoggingService, LoggingService>();
            services.AddScoped<ISecurityLogger, SecurityLogger>();
            services.AddLogging();

            // Register caching service
            services.AddSingleton<ICacheService, RedisCacheService>();

            // Register JWT service
            services.AddScoped<IJwtService, JwtService>();

            return services;
        }
    }
}
