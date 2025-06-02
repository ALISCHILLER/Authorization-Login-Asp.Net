using Authorization_Login_Asp.Net.Core.Application.Common.Behaviors;
using Authorization_Login_Asp.Net.Core.Application.Common.Interfaces;
using Authorization_Login_Asp.Net.Core.Application.Common.Middleware;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Authorization_Login_Asp.Net.Core.Application.Common.Extensions
{
    /// <summary>
    /// متدهای توسعه‌دهنده برای ثبت سرویس‌های برنامه
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// اضافه کردن سرویس‌های مورد نیاز لایه کاربرد
        /// </summary>
        /// <param name="services">کالکشن سرویس‌ها</param>
        /// <param name="configuration">تنظیمات برنامه</param>
        /// <returns>کالکشن سرویس‌ها برای زنجیره‌ای کردن متدها</returns>
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // ثبت Assembly جاری برای استفاده در AutoMapper و FluentValidation
            var assembly = Assembly.GetExecutingAssembly();

            // ثبت MediatR
            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(assembly);
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            });

            // ثبت AutoMapper
            services.AddAutoMapper(assembly);

            // ثبت FluentValidation
            services.AddValidatorsFromAssembly(assembly);

            // ثبت سرویس‌های برنامه
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<ITokenService, TokenService>();

            // ثبت تنظیمات برنامه
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            services.Configure<IdentitySettings>(configuration.GetSection("IdentitySettings"));

            return services;
        }

        /// <summary>
        /// اضافه کردن میدلورهای مورد نیاز برنامه
        /// </summary>
        /// <param name="app">سازنده برنامه</param>
        /// <returns>سازنده برنامه برای زنجیره‌ای کردن متدها</returns>
        public static IApplicationBuilder UseApplicationMiddleware(this IApplicationBuilder app)
        {
            // اضافه کردن میدلور مدیریت خطا
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // اضافه کردن میدلور‌های امنیتی
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            return app;
        }
    }

    /// <summary>
    /// تنظیمات JWT
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// کلید مخفی برای امضای توکن
        /// </summary>
        public string SecretKey { get; set; }

        /// <summary>
        /// مدت زمان اعتبار توکن دسترسی (به دقیقه)
        /// </summary>
        public int AccessTokenExpirationMinutes { get; set; }

        /// <summary>
        /// مدت زمان اعتبار توکن رفرش (به روز)
        /// </summary>
        public int RefreshTokenExpirationDays { get; set; }

        /// <summary>
        /// صادرکننده توکن
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// مصرف‌کننده توکن
        /// </summary>
        public string Audience { get; set; }
    }

    /// <summary>
    /// تنظیمات احراز هویت
    /// </summary>
    public class IdentitySettings
    {
        /// <summary>
        /// حداقل طول رمز عبور
        /// </summary>
        public int MinimumPasswordLength { get; set; }

        /// <summary>
        /// آیا رمز عبور باید شامل حروف بزرگ باشد
        /// </summary>
        public bool RequireUppercase { get; set; }

        /// <summary>
        /// آیا رمز عبور باید شامل حروف کوچک باشد
        /// </summary>
        public bool RequireLowercase { get; set; }

        /// <summary>
        /// آیا رمز عبور باید شامل اعداد باشد
        /// </summary>
        public bool RequireDigit { get; set; }

        /// <summary>
        /// آیا رمز عبور باید شامل کاراکترهای خاص باشد
        /// </summary>
        public bool RequireSpecialCharacter { get; set; }

        /// <summary>
        /// حداکثر تعداد تلاش‌های ناموفق برای ورود
        /// </summary>
        public int MaxFailedLoginAttempts { get; set; }

        /// <summary>
        /// مدت زمان قفل شدن حساب کاربری (به دقیقه)
        /// </summary>
        public int AccountLockoutDurationMinutes { get; set; }

        /// <summary>
        /// آیا احراز هویت دو مرحله‌ای فعال است
        /// </summary>
        public bool RequireTwoFactor { get; set; }
    }
} 