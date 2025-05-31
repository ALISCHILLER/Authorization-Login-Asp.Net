using Authorization_Login_Asp.Net.Domain.Entities;
using Authorization_Login_Asp.Net.Domain.Interfaces;
using Authorization_Login_Asp.Net.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Authorization_Login_Asp.Net.Infrastructure.Data
{
    /// <summary>
    /// کلاس کانتکست دیتابیس اصلی برنامه
    /// </summary>
    public class AppDbContext : DbContext
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<AppDbContext> _logger;

        public AppDbContext(
            DbContextOptions<AppDbContext> options,
            ICurrentUserService currentUserService = null,
            ILogger<AppDbContext> logger = null)
            : base(options)
        {
            _currentUserService = currentUserService;
            _logger = logger;
        }

        /// <summary>
        /// جدول کاربران
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// جدول نقش‌ها
        /// </summary>
        public DbSet<Role> Roles { get; set; }

        /// <summary>
        /// جدول پرمیشن‌ها
        /// </summary>
        public DbSet<Permission> Permissions { get; set; }

        /// <summary>
        /// جدول ارتباط بین نقش‌ها و پرمیشن‌ها
        /// </summary>
        public DbSet<RolePermission> RolePermissions { get; set; }

        /// <summary>
        /// جدول توکن‌های رفرش
        /// </summary>
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        /// <summary>
        /// جدول کدهای بازیابی دو مرحله‌ای
        /// </summary>
        public DbSet<TwoFactorRecoveryCode> RecoveryCodes { get; set; }

        /// <summary>
        /// جدول تاریخچه ورودها
        /// </summary>
        public DbSet<LoginHistory> LoginHistory { get; set; }

        /// <summary>
        /// جدول دستگاه‌های متصل کاربران
        /// </summary>
        public DbSet<UserDevice> UserDevices { get; set; }

        /// <summary>
        /// جدول ارتباط بین کاربران و نقش‌ها
        /// </summary>
        public DbSet<UserRole> UserRoles { get; set; }

        /// <summary>
        /// متد تنظیم پیکربندی مدل‌ها در EF Core
        /// </summary>
        /// <param name="modelBuilder">ابزار ساخت مدل‌های EF Core</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // اعمال پیکربندی‌های جداگانه برای هر موجودیت
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new PermissionConfiguration());
            modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
            modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
            modelBuilder.ApplyConfiguration(new TwoFactorRecoveryCodeConfiguration());
            modelBuilder.ApplyConfiguration(new LoginHistoryConfiguration());
            modelBuilder.ApplyConfiguration(new UserDeviceConfiguration());

            // اعمال فیلترهای سراسری
            ApplyGlobalFilters(modelBuilder);

            // Configure UserRole as many-to-many relationship
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            // Configure other relationships
            modelBuilder.Entity<User>()
                .HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.UserDevices)
                .WithOne(ud => ud.User)
                .HasForeignKey(ud => ud.UserId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.LoginHistory)
                .WithOne(lh => lh.User)
                .HasForeignKey(lh => lh.UserId);

            modelBuilder.Entity<Role>()
                .HasMany(r => r.Permissions)
                .WithOne(rp => rp.Role)
                .HasForeignKey(rp => rp.RoleId);
        }

        /// <summary>
        /// اعمال فیلترهای سراسری برای موجودیت‌ها
        /// </summary>
        private void ApplyGlobalFilters(ModelBuilder modelBuilder)
        {
            // فیلتر حذف نرم برای موجودیت‌هایی که IDeletable را پیاده‌سازی کرده‌اند
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(IDeletable).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var property = Expression.Property(parameter, nameof(IDeletable.IsDeleted));
                    var falseConstant = Expression.Constant(false);
                    var lambda = Expression.Lambda(Expression.Equal(property, falseConstant), parameter);

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }

        /// <summary>
        /// ذخیره تغییرات با اعمال تغییرات خودکار
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // اعمال تغییرات خودکار قبل از ذخیره
            ApplyAuditInfo();
            ApplySoftDelete();

            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // لاگ کردن خطای همزمانی
                _logger?.LogError(ex, "خطا در به‌روزرسانی همزمان داده‌ها: {Message}", ex.Message);
                throw new DbUpdateConcurrencyException("خطا در به‌روزرسانی همزمان داده‌ها", ex);
            }
            catch (DbUpdateException ex)
            {
                // لاگ کردن خطای به‌روزرسانی
                _logger?.LogError(ex, "خطا در به‌روزرسانی داده‌ها: {Message}", ex.Message);
                throw new DbUpdateException("خطا در به‌روزرسانی داده‌ها", ex);
            }
        }

        /// <summary>
        /// اعمال اطلاعات حسابرسی روی موجودیت‌ها
        /// </summary>
        private void ApplyAuditInfo()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is IAuditable && 
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            var currentUserId = _currentUserService?.UserId;
            var currentTime = DateTime.UtcNow;

            foreach (var entry in entries)
            {
                var entity = (IAuditable)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = currentTime;
                    entity.CreatedBy = currentUserId;
                }
                else
                {
                    entity.UpdatedAt = currentTime;
                    entity.UpdatedBy = currentUserId;
                }
            }
        }

        /// <summary>
        /// اعمال حذف نرم روی موجودیت‌ها
        /// </summary>
        private void ApplySoftDelete()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is IDeletable && e.State == EntityState.Deleted);

            foreach (var entry in entries)
            {
                var entity = (IDeletable)entry.Entity;
                entity.IsDeleted = true;
                entity.DeletedAt = DateTime.UtcNow;
                entity.DeletedBy = _currentUserService?.UserId;
                entry.State = EntityState.Modified;
            }
        }

        /// <summary>
        /// شروع یک تراکنش جدید
        /// </summary>
        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            return await Database.BeginTransactionAsync(cancellationToken);
        }

        /// <summary>
        /// اجرای یک عملیات در قالب تراکنش
        /// </summary>
        public async Task<T> ExecuteInTransactionAsync<T>(
            Func<Task<T>> operation,
            CancellationToken cancellationToken = default)
        {
            using var transaction = await BeginTransactionAsync(cancellationToken);
            try
            {
                var result = await operation();
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// اجرای یک عملیات در قالب تراکنش
        /// </summary>
        public async Task ExecuteInTransactionAsync(
            Func<Task> operation,
            CancellationToken cancellationToken = default)
        {
            using var transaction = await BeginTransactionAsync(cancellationToken);
            try
            {
                await operation();
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
