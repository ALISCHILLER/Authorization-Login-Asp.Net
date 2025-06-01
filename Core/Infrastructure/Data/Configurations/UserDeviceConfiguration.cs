using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Data.Configurations
{
    /// <summary>
    /// کلاس پیکربندی موجودیت دستگاه کاربر
    /// این کلاس تنظیمات نگاشت موجودیت UserDevice به جدول پایگاه داده را تعریف می‌کند
    /// </summary>
    public class UserDeviceConfiguration : IEntityTypeConfiguration<UserDevice>
    {
        /// <summary>
        /// متد پیکربندی موجودیت دستگاه کاربر
        /// </summary>
        /// <param name="builder">سازنده پیکربندی موجودیت</param>
        public void Configure(EntityTypeBuilder<UserDevice> builder)
        {
            // تنظیم کلید اصلی
            builder.HasKey(ud => ud.Id);

            // تنظیم نام جدول
            builder.ToTable("UserDevices");

            // تنظیم محدودیت‌های فیلدها
            builder.Property(ud => ud.DeviceId)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(ud => ud.DeviceName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(ud => ud.DeviceType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(ud => ud.OperatingSystem)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(ud => ud.Browser)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(ud => ud.IpAddress)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(ud => ud.LastLoginAt)
                .IsRequired();

            builder.Property(ud => ud.IsTrusted)
                .IsRequired();

            builder.Property(ud => ud.IsBlocked)
                .IsRequired();

            builder.Property(ud => ud.BlockReason)
                .HasMaxLength(200);

            // تنظیم رابطه با کاربر
            builder.HasOne(ud => ud.User)
                .WithMany(u => u.UserDevices)
                .HasForeignKey(ud => ud.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // تنظیم ایندکس‌ها
            builder.HasIndex(ud => ud.DeviceId);
            builder.HasIndex(ud => ud.UserId);
            builder.HasIndex(ud => ud.LastLoginAt);
            builder.HasIndex(ud => new { ud.UserId, ud.DeviceId }).IsUnique();
        }
    }
}