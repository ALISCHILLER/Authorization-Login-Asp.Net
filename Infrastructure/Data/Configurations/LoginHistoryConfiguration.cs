using Authorization_Login_Asp.Net.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authorization_Login_Asp.Net.Infrastructure.Data.Configurations
{
    /// <summary>
    /// کلاس پیکربندی موجودیت تاریخچه ورود
    /// این کلاس تنظیمات نگاشت موجودیت LoginHistory به جدول پایگاه داده را تعریف می‌کند
    /// </summary>
    public class LoginHistoryConfiguration : IEntityTypeConfiguration<LoginHistory>
    {
        /// <summary>
        /// متد پیکربندی موجودیت تاریخچه ورود
        /// </summary>
        /// <param name="builder">سازنده پیکربندی موجودیت</param>
        public void Configure(EntityTypeBuilder<LoginHistory> builder)
        {
            // تنظیم کلید اصلی
            builder.HasKey(lh => lh.Id);

            // تنظیم نام جدول
            builder.ToTable("LoginHistories");

            // تنظیم محدودیت‌های فیلدها
            builder.Property(lh => lh.UserId)
                .IsRequired();

            builder.Property(lh => lh.LoginTime)
                .IsRequired();

            builder.Property(lh => lh.IpAddress)
                .HasMaxLength(50);

            builder.Property(lh => lh.UserAgent)
                .HasMaxLength(500);

            builder.Property(lh => lh.DeviceName)
                .HasMaxLength(100);

            builder.Property(lh => lh.DeviceType)
                .HasMaxLength(50);

            builder.Property(lh => lh.OperatingSystem)
                .HasMaxLength(50);

            builder.Property(lh => lh.BrowserName)
                .HasMaxLength(50);

            builder.Property(lh => lh.BrowserVersion)
                .HasMaxLength(50);

            builder.Property(lh => lh.Country)
                .HasMaxLength(100);

            builder.Property(lh => lh.City)
                .HasMaxLength(100);

            builder.Property(lh => lh.IsSuccessful)
                .IsRequired();

            builder.Property(lh => lh.FailureReason)
                .HasMaxLength(200);

            builder.Property(lh => lh.LogoutTime)
                .IsRequired(false);

            builder.Property(lh => lh.SessionDuration)
                .IsRequired(false);

            // تنظیم رابطه با کاربر
            builder.HasOne(lh => lh.User)
                .WithMany()
                .HasForeignKey(lh => lh.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // تنظیم ایندکس‌ها برای بهبود عملکرد جستجو
            builder.HasIndex(lh => lh.UserId);
            builder.HasIndex(lh => lh.LoginTime);
            builder.HasIndex(lh => lh.IpAddress);
            builder.HasIndex(lh => lh.IsSuccessful);
            builder.HasIndex(lh => new { lh.UserId, lh.LoginTime }); // ایندکس ترکیبی برای جستجوی تاریخچه کاربر
            builder.HasIndex(lh => new { lh.UserId, lh.IsSuccessful }); // ایندکس ترکیبی برای جستجوی ورودهای موفق/ناموفق

            // تنظیم مقادیر پیش‌فرض
            builder.Property(lh => lh.LoginTime)
                .HasDefaultValueSql("GETUTCDATE()");

            // تنظیم نام‌های فارسی برای فیلدها
            builder.Property(lh => lh.UserId).HasComment("شناسه کاربر");
            builder.Property(lh => lh.LoginTime).HasComment("زمان ورود");
            builder.Property(lh => lh.IpAddress).HasComment("آدرس IP");
            builder.Property(lh => lh.UserAgent).HasComment("اطلاعات مرورگر");
            builder.Property(lh => lh.DeviceName).HasComment("نام دستگاه");
            builder.Property(lh => lh.DeviceType).HasComment("نوع دستگاه");
            builder.Property(lh => lh.OperatingSystem).HasComment("سیستم عامل");
            builder.Property(lh => lh.BrowserName).HasComment("نام مرورگر");
            builder.Property(lh => lh.BrowserVersion).HasComment("نسخه مرورگر");
            builder.Property(lh => lh.Country).HasComment("کشور");
            builder.Property(lh => lh.City).HasComment("شهر");
            builder.Property(lh => lh.IsSuccessful).HasComment("وضعیت موفقیت‌آمیز بودن ورود");
            builder.Property(lh => lh.FailureReason).HasComment("دلیل عدم موفقیت");
            builder.Property(lh => lh.LogoutTime).HasComment("زمان خروج");
            builder.Property(lh => lh.SessionDuration).HasComment("مدت زمان حضور کاربر (به ثانیه)");
        }
    }
} 