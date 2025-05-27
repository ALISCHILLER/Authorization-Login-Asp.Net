using Authorization_Login_Asp.Net.Application.Interfaces;
using Authorization_Login_Asp.Net.Infrastructure.Caching;
using Authorization_Login_Asp.Net.Infrastructure.Repositories;
using Authorization_Login_Asp.Net.Infrastructure.Security;
using Authorization_Login_Asp.Net.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Authorization_Login_Asp.Net.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            // Register services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<ILoggingService, LoggingService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ISmsService, SmsService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();

            // Register security services
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ITwoFactorAuthenticator, TwoFactorAuthenticator>();
            services.AddSingleton<RateLimiter>();

            // Register caching service
            services.AddSingleton<ICacheService, RedisCacheService>();

            // Register logging
            services.AddLogging();

            return services;
        }
    }
}
