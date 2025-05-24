using Authorization_Login_Asp.Net.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authorization_Login_Asp.Net.Infrastructure.Data.Configurations
{
    /// <summary>
    /// پیکربندی EF Core برای Entity مدل User
    /// </summary>
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // تعریف کلید اصلی
            builder.HasKey(u => u.Id);

            // پیکربندی ستون نام کاربری
            builder.Property(u => u.Username)
                   .IsRequired()      // مقدار اجباری است
                   .HasMaxLength(50); // حداکثر طول ۵۰ کاراکتر

            // پیکربندی Owned Type برای ایمیل (Email)
            builder.OwnsOne(u => u.Email, email =>
            {
                email.Property(e => e.Value)
                     .HasColumnName("Email")  // نام ستون در دیتابیس
                     .IsRequired()            // اجباری
                     .HasMaxLength(100);      // حداکثر طول ۱۰۰ کاراکتر
            });

            // پیکربندی ستون هش رمز عبور
            builder.Property(u => u.PasswordHash)
                   .IsRequired();

            // پیکربندی ستون نام کامل (اختیاری)
            builder.Property(u => u.FullName)
                   .HasMaxLength(100);

            // پیکربندی ستون نقش کاربر به صورت رشته یا enum
            builder.Property(u => u.Role)
                   .IsRequired()
                   .HasMaxLength(20);

            // تاریخ ایجاد حساب کاربری
            builder.Property(u => u.CreatedAt)
                   .IsRequired();

            // وضعیت فعال بودن حساب (پیش‌فرض true)
            builder.Property(u => u.IsActive)
                   .IsRequired()
                   .HasDefaultValue(true);

            // تعریف رابطه یک به چند با توکن‌های رفرش
            builder.HasMany(u => u.RefreshTokens)
                   .WithOne(rt => rt.User)
                   .HasForeignKey(rt => rt.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // ایندکس یکتا روی نام کاربری
            builder.HasIndex(u => u.Username)
                   .IsUnique();

            // ایندکس یکتا روی ایمیل (ستون Owned Type)
            builder.HasIndex("Email")
                   .IsUnique();
        }
    }
}
