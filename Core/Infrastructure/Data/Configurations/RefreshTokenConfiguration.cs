using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Data.Configurations
{
    /// <summary>
    /// کلاس پیکربندی موجودیت توکن رفرش
    /// این کلاس تنظیمات نگاشت موجودیت RefreshToken به جدول پایگاه داده را تعریف می‌کند
    /// </summary>
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        /// <summary>
        /// متد پیکربندی موجودیت توکن رفرش
        /// </summary>
        /// <param name="builder">سازنده پیکربندی موجودیت</param>
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            // تنظیم کلید اصلی
            builder.HasKey(rt => rt.Id);

            // تنظیم نام جدول
            builder.ToTable("RefreshTokens");

            // تنظیم محدودیت‌های فیلدها
            builder.Property(rt => rt.Token)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(rt => rt.UserId)
                .IsRequired();

            builder.Property(rt => rt.ExpiresAt)
                .IsRequired();

            builder.Property(rt => rt.CreatedAt)
                .IsRequired();

            builder.Property(rt => rt.CreatedByIp)
                .HasMaxLength(50);

            builder.Property(rt => rt.RevokedByIp)
                .HasMaxLength(50);

            builder.Property(rt => rt.ReasonRevoked)
                .HasMaxLength(200);

            // تنظیم رابطه با کاربر
            builder.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // تنظیم رابطه با توکن جایگزین
            builder.HasOne(rt => rt.ReplacedByToken)
                .WithMany()
                .HasForeignKey(rt => rt.ReplacedByTokenId)
                .OnDelete(DeleteBehavior.Restrict);

            // تنظیم ایندکس‌ها
            builder.HasIndex(rt => rt.Token);
            builder.HasIndex(rt => rt.UserId);
            builder.HasIndex(rt => rt.ExpiresAt);
            builder.HasIndex(rt => rt.ReplacedByTokenId);
        }
    }
}