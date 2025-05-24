using Authorization_Login_Asp.Net.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authorization_Login_Asp.Net.Infrastructure.Data.Configurations
{
    /// <summary>
    /// پیکربندی EF Core برای مدل RefreshToken
    /// </summary>
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            // کلید اصلی
            builder.HasKey(rt => rt.Id);

            // ستون توکن رفرش (الزامی)
            builder.Property(rt => rt.Token)
                   .IsRequired();

            // تاریخ ایجاد (الزامی)
            builder.Property(rt => rt.CreatedAt)
                   .IsRequired();

            // تاریخ انقضا (الزامی)
            builder.Property(rt => rt.ExpiresAt)
                   .IsRequired();

            // RevokedAt اختیاری است و نیاز به تنظیم خاص ندارد

            // ReplacedByTokenId اختیاری است و نیاز به تنظیم خاص ندارد

            // تنظیم رابطه خودارجاع برای توکن جایگزین (Token Rotation)
            builder.HasOne(rt => rt.ReplacedByToken)
                   .WithMany() // اینجا چون توکن جایگزین یک مجموعه ندارد، با WithMany() تعریف می‌کنیم
                   .HasForeignKey(rt => rt.ReplacedByTokenId)
                   .OnDelete(DeleteBehavior.Restrict); // حذف محدود به حذف دستی

            // کلید خارجی به کاربر
            builder.HasOne(rt => rt.User)
                   .WithMany(u => u.RefreshTokens) // یک کاربر می‌تواند چند توکن رفرش داشته باشد
                   .HasForeignKey(rt => rt.UserId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade); // حذف کاربر، توکن‌ها را هم حذف کند

            // اندیس روی UserId برای افزایش سرعت جستجو
            builder.HasIndex(rt => rt.UserId);
        }
    }
}
