using Authorization_Login_Asp.Net.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authorization_Login_Asp.Net.Infrastructure.Data.Configurations
{
    /// <summary>
    /// کلاس پیکربندی موجودیت کدهای بازیابی احراز هویت دو مرحله‌ای
    /// </summary>
    public class TwoFactorRecoveryCodeConfiguration : IEntityTypeConfiguration<TwoFactorRecoveryCode>
    {
        /// <summary>
        /// متد پیکربندی موجودیت کدهای بازیابی
        /// </summary>
        /// <param name="builder">سازنده پیکربندی موجودیت</param>
        public void Configure(EntityTypeBuilder<TwoFactorRecoveryCode> builder)
        {
            // تنظیم نام جدول
            builder.ToTable("TwoFactorRecoveryCodes");

            // تنظیم محدودیت‌های فیلدها
            builder.Property(c => c.Code)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(c => c.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(c => c.UsedAt)
                .IsRequired(false);

            builder.Property(c => c.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(c => c.ExpiresAt)
                .IsRequired();

            builder.Property(c => c.IsUsed)
                .IsRequired()
                .HasDefaultValue(false);

            // تنظیم ایندکس‌ها
            builder.HasIndex(c => c.UserId);
            builder.HasIndex(c => c.Code);
            builder.HasIndex(c => c.ExpiresAt);
            builder.HasIndex(c => c.IsActive);
            builder.HasIndex(c => c.IsUsed);

            // تنظیم رابطه با کاربر
            builder.HasOne(c => c.User)
                .WithMany(u => u.RecoveryCodes)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // تنظیم ایندکس ترکیبی برای جلوگیری از تکرار کد برای یک کاربر
            builder.HasIndex(c => new { c.UserId, c.Code })
                .IsUnique();
        }
    }
} 