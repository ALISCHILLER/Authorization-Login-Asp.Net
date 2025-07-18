using Authorization_Login_Asp.Net.Core.Application.Common.Behaviors;


using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Authorization_Login_Asp.Net.Core.Infrastructure.Services; // Required for UserService, UserAuthenticationService etc. if their impl are in Infra
using Authorization_Login_Asp.Net.Core.Infrastructure.Services.Auth; // Required for UserAuthService if its impl is in Infra/Auth

// It's generally better if Application layer does not directly depend on Infrastructure implementations.
// Ideally, these registrations (Interface from App -> Impl from Infra) should happen in Program.cs or an Infrastructure extension method.
// However, if this is the established pattern, we'll follow it for now and add necessary usings.
// For IUserService -> Infrastructure.Services.UserService
// For IUserAuthenticationService -> Infrastructure.Services.Auth.UserAuthService (once created and logic moved)

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
        services.AddScoped<IUserService, Infrastructure.Services.UserService>(); // Explicitly pointing to Infrastructure implementation
        services.AddScoped<IUserAuthenticationService, UserAuthService>(); // Pointing to new UserAuthService in Infrastructure.Services.Auth
        services.AddScoped<Authorization_Login_Asp.Net.Core.Application.Interfaces.Services.ILoginHistoryService, Infrastructure.Services.LoginHistoryService>(); // Pointing to new LoginHistoryService in Infrastructure.Services

        // TODO: Verify UserProfileService and UserAuthorizationService implementation and location
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<IUserAuthorizationService, UserAuthorizationService>();

        // services.AddScoped<ITokenService, TokenService>(); // TokenService class not found in Application/Services
        // services.AddScoped<IErrorHandlingService, ErrorHandlingService>(); // Registered in InfrastructureExtensions

        return services;
    }
}