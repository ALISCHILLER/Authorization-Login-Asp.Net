using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Authorization_Login_Asp.Net.Core.Infrastructure.Middleware;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Extensions
{
    /// <summary>
    /// تنظیمات و پیکربندی middleware‌ها
    /// </summary>
    public static class MiddlewareConfiguration
    {
        /// <summary>
        /// اضافه کردن middleware‌های مورد نیاز به pipeline
        /// </summary>
        public static IApplicationBuilder UseCustomMiddlewares(this IApplicationBuilder app)
        {
            // Middleware‌های امنیتی
            app.UseSecurityHeaders();
            app.UseRateLimiting();

            // Middleware‌های لاگینگ و ردیابی
            app.UseCorrelationId();
            app.UseRequestLogging();
            app.UseResponseLogging();
            app.UseAuditLogging();

            // Middleware‌های مدیریت خطا
            app.UseGlobalExceptionHandling();
            app.UseUnauthorizedHandling();
            app.UseForbiddenHandling();
            app.UseNotFoundHandling();
            app.UseBadRequestHandling();
            app.UseValidationHandling();

            // Middleware‌های بهینه‌سازی
            app.UseResponseCompression();
            app.UseCaching();

            // Middleware‌های متریک‌ها
            app.UseMetrics();

            return app;
        }

        /// <summary>
        /// اضافه کردن سرویس‌های مورد نیاز middleware‌ها
        /// </summary>
        public static IServiceCollection AddCustomMiddlewares(this IServiceCollection services)
        {
            // سرویس‌های امنیتی
            services.AddSecurityHeaders();
            services.AddRateLimiting();

            // سرویس‌های لاگینگ و ردیابی
            services.AddCorrelationId();
            services.AddRequestLogging();
            services.AddResponseLogging();
            services.AddAuditLogging();

            // سرویس‌های مدیریت خطا
            services.AddGlobalExceptionHandling();
            services.AddUnauthorizedHandling();
            services.AddForbiddenHandling();
            services.AddNotFoundHandling();
            services.AddBadRequestHandling();
            services.AddValidationHandling();

            // سرویس‌های بهینه‌سازی
            services.AddResponseCompression();
            services.AddCaching();

            // سرویس‌های متریک‌ها
            services.AddMetrics();

            return services;
        }
    }

    /// <summary>
    /// کلاس‌های کمکی برای تنظیمات middleware‌ها
    /// </summary>
    internal static class MiddlewareExtensions
    {
        // امنیتی
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) =>
            app.UseMiddleware<SecurityHeadersMiddleware>();

        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app) =>
            app.UseMiddleware<RateLimitingMiddleware>();

        // لاگینگ و ردیابی
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app) =>
            app.UseMiddleware<CorrelationIdMiddleware>();

        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app) =>
            app.UseMiddleware<RequestLoggingMiddleware>();

        public static IApplicationBuilder UseResponseLogging(this IApplicationBuilder app) =>
            app.UseMiddleware<ResponseLoggingMiddleware>();

        public static IApplicationBuilder UseAuditLogging(this IApplicationBuilder app) =>
            app.UseMiddleware<AuditLoggingMiddleware>();

        // مدیریت خطا
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app) =>
            app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

        public static IApplicationBuilder UseUnauthorizedHandling(this IApplicationBuilder app) =>
            app.UseMiddleware<UnauthorizedMiddleware>();

        public static IApplicationBuilder UseForbiddenHandling(this IApplicationBuilder app) =>
            app.UseMiddleware<ForbiddenMiddleware>();

        public static IApplicationBuilder UseNotFoundHandling(this IApplicationBuilder app) =>
            app.UseMiddleware<NotFoundMiddleware>();

        public static IApplicationBuilder UseBadRequestHandling(this IApplicationBuilder app) =>
            app.UseMiddleware<BadRequestMiddleware>();

        public static IApplicationBuilder UseValidationHandling(this IApplicationBuilder app) =>
            app.UseMiddleware<RequestValidationMiddleware>();

        // بهینه‌سازی
        public static IApplicationBuilder UseResponseCompression(this IApplicationBuilder app) =>
            app.UseMiddleware<ResponseCompressionMiddleware>();

        public static IApplicationBuilder UseCaching(this IApplicationBuilder app) =>
            app.UseMiddleware<CacheMiddleware>();

        // متریک‌ها
        public static IApplicationBuilder UseMetrics(this IApplicationBuilder app) =>
            app.UseMiddleware<MetricsMiddleware>();

        // سرویس‌ها
        public static IServiceCollection AddSecurityHeaders(this IServiceCollection services) =>
            services.AddScoped<SecurityHeadersMiddleware>();

        public static IServiceCollection AddRateLimiting(this IServiceCollection services) =>
            services.AddScoped<RateLimitingMiddleware>();

        public static IServiceCollection AddCorrelationId(this IServiceCollection services) =>
            services.AddScoped<CorrelationIdMiddleware>();

        public static IServiceCollection AddRequestLogging(this IServiceCollection services) =>
            services.AddScoped<RequestLoggingMiddleware>();

        public static IServiceCollection AddResponseLogging(this IServiceCollection services) =>
            services.AddScoped<ResponseLoggingMiddleware>();

        public static IServiceCollection AddAuditLogging(this IServiceCollection services) =>
            services.AddScoped<AuditLoggingMiddleware>();

        public static IServiceCollection AddGlobalExceptionHandling(this IServiceCollection services) =>
            services.AddScoped<GlobalExceptionHandlingMiddleware>();

        public static IServiceCollection AddUnauthorizedHandling(this IServiceCollection services) =>
            services.AddScoped<UnauthorizedMiddleware>();

        public static IServiceCollection AddForbiddenHandling(this IServiceCollection services) =>
            services.AddScoped<ForbiddenMiddleware>();

        public static IServiceCollection AddNotFoundHandling(this IServiceCollection services) =>
            services.AddScoped<NotFoundMiddleware>();

        public static IServiceCollection AddBadRequestHandling(this IServiceCollection services) =>
            services.AddScoped<BadRequestMiddleware>();

        public static IServiceCollection AddValidationHandling(this IServiceCollection services) =>
            services.AddScoped<RequestValidationMiddleware>();

        public static IServiceCollection AddResponseCompression(this IServiceCollection services) =>
            services.AddScoped<ResponseCompressionMiddleware>();

        public static IServiceCollection AddCaching(this IServiceCollection services) =>
            services.AddScoped<CacheMiddleware>();

        public static IServiceCollection AddMetrics(this IServiceCollection services) =>
            services.AddScoped<MetricsMiddleware>();
    }
}