using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Data.Configurations
{
    /// <summary>
    /// کلاس پیکربندی موجودیت کاربر
    /// این کلاس تنظیمات نگاشت موجودیت User به جدول پایگاه داده را تعریف می‌کند
    /// </summary>
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        /// <summary>
        /// متد پیکربندی موجودیت کاربر
        /// </summary>
        /// <param name="builder">سازنده پیکربندی موجودیت</param>
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // تنظیم نام جدول
            builder.ToTable("Users");

            // تنظیم محدودیت‌های فیلدها
            builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.PhoneNumber)
                .HasMaxLength(15);

            builder.Property(u => u.FullName)
                .HasMaxLength(100);

            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.ProfileImageUrl)
                .HasMaxLength(500);

            builder.Property(u => u.LastLoginAt)
                .IsRequired(false);

            builder.Property(u => u.LastPasswordChange)
                .IsRequired(false);

            builder.Property(u => u.IsActive)
                .IsRequired();

            builder.Property(u => u.AccountLockoutEnd)
                .IsRequired(false);

            builder.Property(u => u.TwoFactorEnabled)
                .IsRequired();

            builder.Property(u => u.TwoFactorSecret)
                .HasMaxLength(32);

            builder.Property(u => u.TwoFactorType)
                .IsRequired(false);

            builder.Property(u => u.FailedLoginAttempts)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(u => u.IsEmailVerified)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(u => u.IsPhoneVerified)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(u => u.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // تنظیم ایندکس‌ها
            builder.HasIndex(u => u.Username).IsUnique();
            builder.HasIndex(u => u.Email).IsUnique();
            builder.HasIndex(u => u.PhoneNumber).IsUnique();
            builder.HasIndex(u => u.IsActive);
            builder.HasIndex(u => u.LastLoginAt);
            builder.HasIndex(u => u.CreatedAt);

            // تنظیم رابطه با نقش
            //builder.HasOne(u => u.Role)
            //    .WithMany()
            //    .HasForeignKey(u => u.RoleId)
            //    .OnDelete(DeleteBehavior.Restrict);



            // تنظیم رابطه با دستگاه‌ها
            builder.HasMany(u => u.UserDevices)
                .WithOne(d => d.User)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // تنظیم رابطه با توکن‌های رفرش
            builder.HasMany(u => u.RefreshTokens)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // تنظیم رابطه با تاریخچه ورود
            builder.HasMany(u => u.LoginHistory)
                .WithOne(h => h.User)
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // تنظیم رابطه با کدهای بازیابی دو مرحله‌ای
            builder.HasMany(u => u.RecoveryCodes)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // تنظیم رابطه با تنظیمات امنیتی
            builder.OwnsOne(u => u.SecuritySettings, securitySettings =>
            {
                securitySettings.Property(s => s.PasswordSalt)
                    .HasMaxLength(32);
                securitySettings.Property(s => s.PasswordExpiryDate)
                    .IsRequired(false);
            });
        }
    }
}