using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Data.Configurations
{
    /// <summary>
    /// کلاس پیکربندی موجودیت نقش
    /// این کلاس تنظیمات نگاشت موجودیت Role به جدول پایگاه داده را تعریف می‌کند
    /// </summary>
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        /// <summary>
        /// متد پیکربندی موجودیت نقش
        /// </summary>
        /// <param name="builder">سازنده پیکربندی موجودیت</param>
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            // تنظیم کلید اصلی
            builder.HasKey(r => r.Id);

            // تنظیم نام جدول
            builder.ToTable("Roles");

            // تنظیم محدودیت‌های فیلدها
            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(r => r.DisplayName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(r => r.Description)
                .HasMaxLength(500);

            builder.Property(r => r.Type)
                .IsRequired();

            builder.Property(r => r.IsActive)
                .IsRequired();

            builder.Property(r => r.IsSystem)
                .IsRequired();

            // تنظیم ایندکس‌ها
            builder.HasIndex(r => r.Name).IsUnique();
            builder.HasIndex(r => r.Type);
            builder.HasIndex(r => r.IsActive);
            builder.HasIndex(r => r.IsSystem);

            // تنظیم رابطه با کاربران
            builder.HasMany(r => r.Users)
                .WithMany(u => u.Roles)
                .UsingEntity(j => j.ToTable("UserRoles"));

            // تنظیم رابطه با دسترسی‌ها
            builder.HasMany(r => r.RolePermissions)
                .WithOne(rp => rp.Role)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
