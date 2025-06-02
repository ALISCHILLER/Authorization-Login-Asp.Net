# Application Layer

این لایه مسئول پیاده‌سازی منطق کسب و کار و هماهنگی بین لایه‌های مختلف برنامه است.

## ساختار پوشه‌ها

### Common
- **Behaviors**: رفتارهای خط لوله مانند اعتبارسنجی، لاگینگ و ...
- **Extensions**: متدهای توسعه‌دهنده
- **Interfaces**: رابط‌های مشترک
- **Mappings**: پروفایل‌های AutoMapper
- **Middleware**: میدلورهای برنامه
- **Validators**: اعتبارسنج‌های مشترک

### DTOs
- **Auth**: DTOهای مربوط به احراز هویت
- **Common**: DTOهای مشترک
- **Roles**: DTOهای مربوط به نقش‌ها
- **Users**: DTOهای مربوط به کاربران

### Exceptions
- **Domain**: استثناهای مربوط به دامنه
- **Application**: استثناهای مربوط به لایه کاربرد

### Extensions
- **ServiceCollectionExtensions**: ثبت سرویس‌ها
- **ApplicationBuilderExtensions**: تنظیمات برنامه

### Features
- **Auth**: ویژگی‌های احراز هویت
- **Users**: ویژگی‌های مدیریت کاربران
- **Roles**: ویژگی‌های مدیریت نقش‌ها

### Filters
- **Action**: فیلترهای اکشن
- **Exception**: فیلترهای استثنا

### Interfaces
- **Services**: رابط‌های سرویس
- **Repositories**: رابط‌های مخزن

### Services
- **Auth**: سرویس‌های احراز هویت
- **Users**: سرویس‌های کاربران
- **Roles**: سرویس‌های نقش‌ها

### Validators
- **Auth**: اعتبارسنج‌های احراز هویت
- **Users**: اعتبارسنج‌های کاربران
- **Roles**: اعتبارسنج‌های نقش‌ها

## اصول طراحی

1. **اصل مسئولیت واحد (SRP)**
   - هر کلاس و رابط باید یک مسئولیت مشخص داشته باشد
   - جداسازی منطقی سرویس‌ها و رابط‌ها

2. **اصل وارونگی وابستگی (DIP)**
   - وابستگی به انتزاع‌ها، نه پیاده‌سازی‌ها
   - استفاده از تزریق وابستگی

3. **اصل جداسازی رابط (ISP)**
   - رابط‌های کوچک و متمرکز
   - جلوگیری از رابط‌های چاق

4. **اصل باز/بسته (OCP)**
   - قابلیت گسترش بدون تغییر کد موجود
   - استفاده از الگوهای طراحی مناسب

## بهترین شیوه‌ها

1. **اعتبارسنجی**
   - استفاده از FluentValidation
   - اعتبارسنجی در لایه کاربرد
   - پیام‌های خطای مناسب

2. **لاگینگ**
   - لاگینگ مناسب عملیات‌ها
   - ثبت خطاها و استثناها
   - استفاده از سطوح مختلف لاگ

3. **امنیت**
   - اعتبارسنجی ورودی‌ها
   - بررسی مجوزها
   - رمزنگاری داده‌های حساس

4. **کارایی**
   - استفاده از کش
   - بهینه‌سازی کوئری‌ها
   - مدیریت منابع

5. **تست‌پذیری**
   - قابلیت تست واحد
   - استفاده از Mock
   - جداسازی منطقی

## نحوه استفاده

1. **ثبت سرویس‌ها**
```csharp
services.AddApplicationServices();
```

2. **استفاده از سرویس‌ها**
```csharp
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    
    public UserController(IUserService userService)
    {
        _userService = userService;
    }
}
```

3. **استفاده از DTOها**
```csharp
public class CreateUserRequest
{
    [Required]
    public string Username { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
```

4. **استفاده از اعتبارسنج‌ها**
```csharp
public class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
} 