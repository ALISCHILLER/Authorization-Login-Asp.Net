using Authorization_Login_Asp.Net.Domain.Entities;
using Authorization_Login_Asp.Net.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Authorization_Login_Asp.Net.Infrastructure.Data
{
    /// <summary>
    /// کلاس DbContext اصلی برنامه که نماینده دیتابیس و مدل‌های آن است
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// جدول کاربران (Users)
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// جدول نقش‌ها (Roles)
        /// </summary>
        public DbSet<Role> Roles { get; set; }

        /// <summary>
        /// جدول دسترسی‌ها (Permissions)
        /// </summary>
        public DbSet<Permission> Permissions { get; set; }

        /// <summary>
        /// جدول نقش-پرمیشن (RolePermissions)
        /// ارتباط بین نقش‌ها و دسترسی‌ها
        /// </summary>
        public DbSet<RolePermission> RolePermissions { get; set; }

        /// <summary>
        /// جدول توکن‌های رفرش (RefreshTokens)
        /// برای مدیریت توکن‌های رفرش در احراز هویت
        /// </summary>
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        /// <summary>
        /// متد تنظیم پیکربندی مدل‌ها در EF Core
        /// </summary>
        /// <param name="modelBuilder">ابزار ساخت مدل‌های EF Core</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // اعمال پیکربندی‌های جداگانه هر Entity
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new PermissionConfiguration());
            modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
            modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        }
    }
}
