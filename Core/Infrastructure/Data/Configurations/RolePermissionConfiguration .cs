using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Data.Configurations
{
    /// <summary>
    /// پیکربندی EF Core برای جدول RolePermission
    /// </summary>
    public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            // کلید اصلی
            builder.HasKey(rp => rp.Id);

            // کلید خارجی نقش (RoleId) - اجباری
            builder.Property(rp => rp.RoleId)
                   .IsRequired();

            // کلید خارجی پرمیشن (PermissionId) - اجباری
            builder.Property(rp => rp.PermissionId)
                   .IsRequired();

            // توضیحات اختیاری
            builder.Property(rp => rp.Description)
                   .HasMaxLength(200);

            // تعریف رابطه با Role
            builder.HasOne(rp => rp.Role)
                   .WithMany(r => r.RolePermissions)
                   .HasForeignKey(rp => rp.RoleId)
                   .OnDelete(DeleteBehavior.Cascade);

            // تعریف رابطه با Permission
            builder.HasOne(rp => rp.Permission)
                   .WithMany(p => p.RolePermissions)
                   .HasForeignKey(rp => rp.PermissionId)
                   .OnDelete(DeleteBehavior.Cascade);

            // ایندکس ترکیبی برای جلوگیری از تکرار نقش-پرمیشن
            builder.HasIndex(rp => new { rp.RoleId, rp.PermissionId })
                   .IsUnique();
        }
    }
}
