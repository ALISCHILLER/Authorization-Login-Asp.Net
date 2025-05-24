using Authorization_Login_Asp.Net.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authorization_Login_Asp.Net.Infrastructure.Data.Configurations
{
    /// <summary>
    /// پیکربندی EF Core برای جدول Role
    /// </summary>
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            // کلید اصلی
            builder.HasKey(r => r.Id);

            // نام نقش - اجباری و یکتا
            builder.Property(r => r.Name)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.HasIndex(r => r.Name).IsUnique(); // اطمینان از یکتایی نقش‌ها

            // توضیحات - اختیاری
            builder.Property(r => r.Description)
                   .HasMaxLength(200);

            // ارتباط یک به چند: یک نقش می‌تواند چند RolePermission داشته باشد
            builder.HasMany(r => r.RolePermissions)
                   .WithOne(rp => rp.Role)
                   .HasForeignKey(rp => rp.RoleId)
                   .OnDelete(DeleteBehavior.Cascade); // حذف نقش باعث حذف RolePermission‌های مربوطه می‌شود
        }
    }
}
