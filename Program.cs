using Authorization_Login_Asp.Net.Api;
using Authorization_Login_Asp.Net.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.AspNetCore.Diagnostics.HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks.UI.InMemory.Storage;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// اضافه کردن سرویس‌های پایه
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();

// تنظیمات CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("AppSettings:SecuritySettings:CorsSettings:AllowedOrigins").Get<string[]>())
              .WithMethods(builder.Configuration.GetSection("AppSettings:SecuritySettings:CorsSettings:AllowedMethods").Get<string[]>())
              .WithHeaders(builder.Configuration.GetSection("AppSettings:SecuritySettings:CorsSettings:AllowedHeaders").Get<string[]>())
              .AllowCredentials()
              .SetIsOriginAllowed(origin => 
              {
                  var allowedOrigins = builder.Configuration.GetSection("AppSettings:SecuritySettings:CorsSettings:AllowedOrigins").Get<string[]>();
                  return allowedOrigins.Contains(origin);
              });
    });
});

// تنظیمات Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Auth API", 
        Version = "v1",
        Description = "API for Authentication and Authorization",
        Contact = new OpenApiContact
        {
            Name = "Support Team",
            Email = "support@example.com"
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// تنظیمات دیتابیس
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });
});

// تنظیمات احراز هویت
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["AppSettings:JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["AppSettings:JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:JwtSettings:SecretKey"])),
            ClockSkew = TimeSpan.Zero
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
            }
        };
    });

// تنظیمات مجوزها
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireUserRole", policy => policy.RequireRole("User"));
    options.AddPolicy("RequireVerifiedEmail", policy => policy.RequireClaim("EmailVerified", "true"));
});

// تنظیمات کش
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = builder.Configuration["AppSettings:CacheSettings:RedisInstanceName"];
    options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions
    {
        EndPoints = { builder.Configuration.GetConnectionString("Redis") },
        ConnectTimeout = 5000,
        SyncTimeout = 5000,
        AbortOnConnectFail = false,
        AllowAdmin = true,
        ReconnectRetryPolicy = new LinearRetry(1000),
        ConnectRetry = 3,
        DefaultDatabase = 0
    };
});

// تنظیمات Health Checks
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    .AddRedis(builder.Configuration.GetConnectionString("Redis"))
    .AddUrlGroup(new Uri(builder.Configuration["ExternalServices:PaymentGateway"]), "Payment Gateway")
    .AddUrlGroup(new Uri(builder.Configuration["ExternalServices:EmailService"]), "Email Service");

builder.Services.AddHealthChecksUI(options =>
{
    options.SetEvaluationTimeInSeconds(15);
    options.SetMinimumSecondsBetweenFailureNotifications(60);
}).AddInMemoryStorage();

// تنظیمات Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        var username = context.User.Identity?.Name;
        var key = $"{ipAddress}_{username}";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: key,
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = builder.Configuration.GetValue<int>("AppSettings:SecuritySettings:RateLimitSettings:MaxAttempts"),
                Window = TimeSpan.FromMinutes(builder.Configuration.GetValue<int>("AppSettings:SecuritySettings:RateLimitSettings:WindowMinutes"))
            });
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("Rate limit exceeded for {IP}", context.HttpContext.Connection.RemoteIpAddress);
        
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Too many requests",
            retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter) ? retryAfter.TotalSeconds : null
        });
    };
});

// تنظیمات امنیتی اضافی
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = "XSRF-TOKEN";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// تنظیمات HSTS
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
    options.Preload = true;
});

// تنظیمات Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "image/svg+xml", "application/json", "text/json" });
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

// تنظیمات Kestrel
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxConcurrentConnections = builder.Configuration.GetValue<int>("AppSettings:DatabaseSettings:MaxPoolSize");
    options.Limits.MaxConcurrentUpgradedConnections = builder.Configuration.GetValue<int>("AppSettings:DatabaseSettings:MaxPoolSize");
    options.Limits.MaxRequestBodySize = builder.Configuration.GetValue<int>("AppSettings:SecuritySettings:RateLimitSettings:MaxRequestBodySize");
    options.Limits.RequestQueueLimit = builder.Configuration.GetValue<int>("AppSettings:SecuritySettings:RateLimitSettings:RequestQueueSize");
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);
    options.Limits.MaxRequestBufferSize = 1024 * 1024; // 1 MB
    options.Limits.MaxRequestLineSize = 8 * 1024; // 8 KB
    options.Limits.MaxResponseBufferSize = 64 * 1024; // 64 KB
    options.Limits.MaxRequestHeadersTotalSize = 32 * 1024; // 32 KB
});

// تنظیمات Dependency Injection
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<ITwoFactorAuthenticator, TwoFactorAuthenticator>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<ICacheService, RedisCacheService>();
builder.Services.AddScoped<ILoggingService, LoggingService>();

// تنظیمات AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// تنظیمات FluentValidation
builder.Services.AddFluentValidation(fv => 
{
    fv.RegisterValidatorsFromAssemblyContaining<Program>();
    fv.AutomaticValidationEnabled = true;
});

// تنظیمات MediatR
builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
});

// تنظیمات میدلورها
var middlewareConfig = new MiddlewareConfiguration();
builder.Services.AddSingleton(middlewareConfig);

// ایجاد برنامه
var app = builder.Build();

// تنظیمات میدلورها
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth API V1");
        c.RoutePrefix = string.Empty;
    });
    app.UseDeveloperExceptionPage();
}

// میدلورهای امنیتی
app.UseHttpsRedirection();
app.UseCors("DefaultCorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

// میدلورهای عملکردی
app.UseResponseCompression();
app.UseRateLimiter();

// میدلورهای لاگینگ و خطا
app.UseExceptionHandler("/error");
app.UseRequestLogging();
app.UseResponseLogging();

// میدلورهای Health Check
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapHealthChecksUI();

// تنظیمات میدلورها در زمان اجرا
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestValidationMiddleware>(middlewareConfig);
app.UseMiddleware<ResponseCompressionMiddleware>(middlewareConfig);
app.UseMiddleware<AuditLoggingMiddleware>(middlewareConfig);
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ResponseLoggingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<CacheMiddleware>();
app.UseMiddleware<NotFoundMiddleware>();

// میدلورهای کنترلر
app.MapControllers();

// اجرای برنامه
app.Run();
