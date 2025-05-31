using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.ResponseCompression;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Threading.RateLimiting;
using Authorization_Login_Asp.Net.Application.Interfaces;
using Authorization_Login_Asp.Net.Infrastructure.Services;
using Authorization_Login_Asp.Net.API.Middlewares;
using Authorization_Login_Asp.Net.Infrastructure.Middleware;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Authorization_Login_Asp.Net.Infrastructure.Security;
using Authorization_Login_Asp.Net.Infrastructure.Options;
using Authorization_Login_Asp.Net.Infrastructure.HealthChecks;
using System.IdentityModel.Tokens.Jwt;
using HealthChecks.UI.Client;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

// تنظیمات اصلی برنامه
var builder = WebApplication.CreateBuilder(args);

// تنظیمات Serilog برای لاگینگ
builder.Host.UseSerilog();

// تنظیمات کنترلرها و JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
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
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<ILoginHistoryService, LoginHistoryService>();
builder.Services.AddMemoryCache();

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
    options.InstanceName = "Authorization.Login:";
});

// ثبت سرویس‌های برنامه
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<ILoggingService, LoggingService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<IMetricsService, MetricsService>();

// تنظیمات AutoMapper برای نگاشت اشیاء
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// تنظیمات FluentValidation برای اعتبارسنجی
builder.Services.AddFluentValidation(fv =>
{
    fv.RegisterValidatorsFromAssemblyContaining<Program>();
    fv.AutomaticValidationEnabled = true;
});

// Configure image service options
builder.Services.Configure<ImageServiceOptions>(builder.Configuration.GetSection("ImageService"));

// تنظیمات Health Checks برای نظارت بر سلامت سیستم
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>()
    .AddRedis(builder.Configuration.GetConnectionString("Redis"))
    .AddUrlGroup(new Uri(builder.Configuration["ExternalServices:ApiEndpoint"]), "External API");

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
});

// Add OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
        tracerProviderBuilder
            .AddSource("Authorization.Login")
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService("Authorization.Login"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSqlClientInstrumentation()
            .AddOtlpExporter());

// ایجاد برنامه
var app = builder.Build();

// تنظیمات میدلورها در محیط توسعه
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

// میدلورهای امنیتی و پایه
app.UseHttpsRedirection();
app.UseCors("DefaultCorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

// میدلورهای لاگینگ و ردیابی
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<MetricsMiddleware>();

// تنظیمات مسیریابی
app.MapControllers();
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(x => new
            {
                name = x.Key,
                status = x.Value.Status.ToString(),
                description = x.Value.Description,
                duration = x.Value.Duration.ToString()
            })
        };
        await context.Response.WriteAsJsonAsync(response);
    }
});

app.MapHealthChecksUI(options =>
{
    options.UIPath = "/health-ui";
    options.ApiPath = "/health-api";
});

app.MapControllers().RequireRateLimiting("fixed");

// اجرای برنامه
app.Run();
