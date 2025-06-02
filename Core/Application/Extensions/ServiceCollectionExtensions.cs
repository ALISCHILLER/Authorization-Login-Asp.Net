using Authorization_Login_Asp.Net.Core.Application.Common.Behaviors;

using Authorization_Login_Asp.Net.Core.Application.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Authorization_Login_Asp.Net.Core.Application.Extensions;

/// <summary>
/// متدهای توسعه‌دهنده برای ثبت سرویس‌های لایه کاربرد
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// ثبت سرویس‌های لایه کاربرد
    /// </summary>
    /// <param name="services">کالکشن سرویس‌ها</param>
    /// <returns>کالکشن سرویس‌ها</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // ثبت MediatR
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        });

        // ثبت FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        // ثبت AutoMapper
        services.AddAutoMapper(assembly);

        // ثبت سرویس‌ها
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserAuthenticationService, UserAuthenticationService>();
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<IUserAuthorizationService, UserAuthorizationService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IErrorHandlingService, ErrorHandlingService>();

        return services;
    }
} 