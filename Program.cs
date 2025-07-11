using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.ResponseCompression;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.IdentityModel.Tokens.Jwt;
using HealthChecks.UI.Client;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;
using OpenTelemetry.Exporter;
using AspNetCoreRateLimit;
using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Application.Validators;
using Authorization_Login_Asp.Net.Core.Infrastructure.Configurations;
using Authorization_Login_Asp.Net.Core.Infrastructure.Data;
using Authorization_Login_Asp.Net.Core.Infrastructure.HealthChecks;
using Authorization_Login_Asp.Net.Core.Infrastructure.Middleware;
using Authorization_Login_Asp.Net.Core.Infrastructure.Options;
using Authorization_Login_Asp.Net.Core.Infrastructure.Security;
using Authorization_Login_Asp.Net.Core.Infrastructure.Services;
using Authorization_Login_Asp.Net.Core.Infrastructure.Telemetry;
using Authorization_Login_Asp.Net.Presentation.Api.Middleware;

// تنظیمات اصلی برنامه
var builder = WebApplication.CreateBuilder(args);

// تنظیمات Serilog برای لاگینگ
builder.Host.UseSerilog();

// تنظیمات کنترلرها و JSON
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
    options.Filters.Add<ValidationExceptionFilter>();
})
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    })
    .AddFluentValidation(fv => 
    {
        fv.RegisterValidatorsFromAssemblyContaining<Program>();
        fv.DisableDataAnnotationsValidation = true;
    });

// تنظیمات Swagger و API Explorer
builder.Services.AddEndpointsApiExplorer();

// تنظیمات CORS برای دسترسی از دامنه‌های مختلف
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>())
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ثبت سرویس‌های JWT
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("AppSettings:JwtSettings"));
// IJwtService is covered by AddInfrastructureServices
// ILoginHistoryService is covered by AddInfrastructureServices (as AuthenticationService)
// MemoryCache is covered by AddInfrastructureServices

// سرویس‌های زیر یا در فایل‌های اکستنشن مربوطه ثبت شده‌اند یا پیاده‌سازی آن‌ها یافت نشد.
// IJwtTokenGenerator, ICurrentUserService, IAuditService, IDomainEventDispatcher
// به همراه IRoleManagementService, IUserManagementService, IPasswordService, IDeviceManagementService
// در مراحل بعدی و پس از بررسی دقیق‌تر فایل‌های اکستنشن، در صورت نیاز به Program.cs بازگردانده یا به فایل اکستنشن صحیح منتقل می‌شوند.
// فعلا برای تمیز ماندن Program.cs، آن‌ها را اینجا نگه نمی‌داریم.

// 注册 HttpContextAccessor
builder.Services.AddHttpContextAccessor(); // Keep, common utility

// Repositories are covered by AddInfrastructureServices
// ILoggingService and ITracingService are covered by AddInfrastructureServices

// سرویس‌هایی که در ادامه به صورت دستی ثبت می‌شوند، آنهایی هستند که هنوز به فایل‌های اکستنشن منتقل نشده‌اند یا نیاز به بررسی بیشتر دارند.

// تنظیمات احراز هویت JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("AppSettings:JwtSettings").Get<JwtSettings>();
    var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        RequireExpirationTime = true,
        ValidateTokenReplay = jwtSettings.RevokeOldTokens
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            // اعتبارسنجی اضافی توکن
            var jwtService = context.HttpContext.RequestServices.GetRequiredService<IJwtService>();
            var token = context.SecurityToken as JwtSecurityToken;
            
            if (token != null)
            {
                try
                {
                    var claims = jwtService.ValidateToken(token.RawData);
                    // اعتبارسنجی‌های اضافی را اینجا انجام دهید
                }
                catch (Exception ex)
                {
                    context.Fail(ex);
                }
            }
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            // پشتیبانی از توکن در کوئری استرینگ
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// تنظیمات Redis برای کش
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "AuthApp:";
}); // Keep, specific Redis config

// ICacheService is covered by AddInfrastructureServices
// IUserService, IEmailService, ISmsService, IImageService, IMetricsService (as Scoped) are covered by AddInfrastructureServices
// ILoggingService (as LoggingAndErrorHandlingService) is covered by AddInfrastructureServices
// ITwoFactorService is covered by AddInfrastructureServices
// AutoMapper and FluentValidation are covered by AddApplicationServices

// سرویس IPasswordHasher به فایل Core/Infrastructure/Extensions/ServiceCollectionExtensions.cs منتقل خواهد شد.
// builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// Configure image service options
builder.Services.Configure<ImageServiceOptions>(builder.Configuration.GetSection("ImageService"));

// تنظیمات Health Checks برای نظارت بر سلامت سیستم
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>()
    .AddRedis(builder.Configuration.GetConnectionString("Redis"))
    .AddUrlGroup(new Uri(builder.Configuration["ExternalServices:ApiEndpoint"]), "External API")
    .AddCheck<DatabaseHealthCheck>("Database");

// تنظیمات UI برای Health Checks
builder.Services.AddHealthChecksUI(options =>
{
    options.SetEvaluationTimeInSeconds(15);
    options.MaximumHistoryEntriesPerEndpoint(50);
    options.SetApiMaxActiveRequests(1);
    options.AddHealthCheckEndpoint("API Health", "/health");
})
.AddInMemoryStorage();

// تنظیمات Rate Limiting برای جلوگیری از حملات
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        var username = context.User.Identity?.Name;
        var key = $"{ipAddress}_{username}";
        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            AutoReplenishment = true,
            PermitLimit = 100,
            Window = TimeSpan.FromMinutes(1)
        });
    });
});

// تنظیمات فشرده‌سازی پاسخ‌ها
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

// تنظیمات Kestrel برای بهینه‌سازی سرور
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxConcurrentConnections = 100;
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);
    
    options.ConfigureHttpsDefaults(https =>
    {
        https.SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | 
                            System.Security.Authentication.SslProtocols.Tls13;
    });
});

// Configure OpenTelemetry
builder.Services.Configure<TracingSettings>(builder.Configuration.GetSection("Tracing")); // Keep, options config
// ITracingService is covered by AddInfrastructureServices

var tracingSettings = builder.Configuration.GetSection("Tracing").Get<TracingSettings>();
if (tracingSettings != null && tracingSettings.EnableTracing)
{
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource
            .AddService(serviceName: "AuthorizationLoginService"))
        .WithTracing(tracing => tracing
            .AddSource("UserService")
            .AddConsoleExporter()
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation())
        .WithMetrics(metrics => metrics
            .AddConsoleExporter()
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation());
}

// Add security headers middleware
builder.Services.AddSecurityHeaders(policies =>
    policies
        .AddDefaultSecurityHeaders()
        .AddStrictTransportSecurityMaxAgeIncludeSubDomains()
        .RemoveServerHeader()
        .AddContentSecurityPolicy(csp =>
        {
            csp.DefaultSources(s => s.Self());
            csp.ScriptSources(s => s.Self().UnsafeInline().UnsafeEval());
            csp.StyleSources(s => s.Self().UnsafeInline());
            csp.ImageSources(s => s.Self().Data());
            csp.FontSources(s => s.Self());
            csp.ConnectSources(s => s.Self());
            csp.FrameSources(s => s.None());
            csp.ObjectSources(s => s.None());
        }));

// Add Prometheus metrics
builder.Services.AddMetrics();
builder.Services.AddPrometheusGatewayPublisher(options =>
{
    options.Endpoint = new Uri(builder.Configuration["Metrics:PrometheusEndpoint"] ?? "http://localhost:9091");
    options.Job = "auth-service";
});

// Register services - These specific services were not found or their registration is handled by extension methods.
// builder.Services.AddScoped<IRoleManagementService, RoleManagementService>();
// builder.Services.AddScoped<IUserManagementService, UserManagementService>();
// builder.Services.AddScoped<IPasswordService, PasswordService>();
// builder.Services.AddScoped<IDeviceManagementService, DeviceManagementService>();

// Call service registration extension methods
builder.Services.AddApplicationServices(); // From Core.Application.Extensions
builder.Services.AddInfrastructureServices(builder.Configuration); // From Core.Infrastructure.Extensions

var app = builder.Build();

// Initialize ServiceLocator
ServiceLocator.Initialize(app.Services);

// Proper middleware ordering
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Security and monitoring middleware
app.UseSecurityHeaders();
app.UseHsts();
app.UseHttpsRedirection();

// Global Error Handling Middleware should be one of the first to catch all subsequent errors.
app.UseMiddleware<ErrorHandlingMiddleware>(); // Moved Presentation.Api.Middleware.ErrorHandlingMiddleware here

// Security and monitoring middleware
// app.UseMiddleware<ExceptionHandlingMiddleware>(); // Already removed

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<MetricsMiddleware>(); // Metrics can be after logging but before auth/CORS if they don't depend on user context

// Then security features that might deny requests
app.UseRateLimiter();
app.UseCors("DefaultCorsPolicy"); // CORS should be before Authentication/Authorization

app.UseAuthentication();
app.UseAuthorization();

// Other features
app.UseResponseCompression();

// Then endpoints
app.MapControllers();
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapHealthChecksUI(options =>
{
    options.UIPath = "/health-ui";
    options.ApiPath = "/health-api";
});

// اجرای برنامه
app.Run();