using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Data.Configurations
{
    /// <summary>
    /// پیکربندی EF Core برای جدول Permission
    /// </summary>
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            // کلید اصلی
            builder.HasKey(p => p.Id);

            // نام پرمیشن - یکتا و اجباری
            builder.Property(p => p.Name)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.HasIndex(p => p.Name).IsUnique(); // ایندکس یکتا برای جلوگیری از تکرار پرمیشن‌ها

            // توضیحات - اختیاری
            builder.Property(p => p.Description)
                   .HasMaxLength(200);

            // Enum نوع پرمیشن - اجباری
            builder.Property(p => p.Type)
                   .IsRequired()
                   .HasConversion<string>() // ذخیره enum به صورت string در دیتابیس
                   .HasMaxLength(50);

            // ارتباط با جدول RolePermission
            builder.HasMany(p => p.RolePermissions)
                   .WithOne(rp => rp.Permission)
                   .HasForeignKey(rp => rp.PermissionId)
                   .OnDelete(DeleteBehavior.Cascade); // حذف پرمیشن باعث حذف RolePermission‌های مرتبط می‌شود
        }
    }
}
