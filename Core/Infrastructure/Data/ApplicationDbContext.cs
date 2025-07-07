using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Authorization_Login_Asp.Net.Core.Domain.Common;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Infrastructure.Data.Configurations;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Data
{
    /// <summary>
    /// کانتکست اصلی دیتابیس برنامه
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTimeService _dateTimeService;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ICurrentUserService currentUserService,
            IDateTimeService dateTimeService) : base(options)
        {
            _currentUserService = currentUserService;
            _dateTimeService = dateTimeService;
        }

        #region DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<LoginHistory> LoginHistory { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // اعمال تنظیمات موجودیت‌ها
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new PermissionConfiguration());
            modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
            modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
            modelBuilder.ApplyConfiguration(new UserPermissionConfiguration());
            modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
            modelBuilder.ApplyConfiguration(new LoginHistoryConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationConfiguration());
            modelBuilder.ApplyConfiguration(new AuditLogConfiguration());

            // اعمال فیلترهای سراسری
            modelBuilder.ApplyGlobalFilters();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId; // Get current user's ID (string)
            var now = _dateTimeService.Now; // Get current time

            foreach (var entry in ChangeTracker.Entries<BaseEntity>()) // Changed from AuditableEntity to BaseEntity
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        // BaseEntity constructor already sets CreatedAt.
                        // We only need to set CreatedBy if it's not set by the entity's MarkAsCreated method.
                        // If MarkAsCreated was called with a userId, this might override it or be redundant.
                        // For consistency, DbContext should be the authority for these audit fields during save.
                        entry.Property(nameof(BaseEntity.CreatedBy)).CurrentValue = userId;
                        entry.Property(nameof(BaseEntity.CreatedAt)).CurrentValue = now; // Ensure it uses DateTimeService
                        entry.Property(nameof(BaseEntity.UpdatedAt)).CurrentValue = null;
                        entry.Property(nameof(BaseEntity.UpdatedBy)).CurrentValue = null;
                        entry.Property(nameof(BaseEntity.DeletedBy)).CurrentValue = null;
                        entry.Property(nameof(BaseEntity.DeletedAt)).CurrentValue = null;
                        entry.Property(nameof(BaseEntity.IsDeleted)).CurrentValue = false;
                        break;

                    case EntityState.Modified:
                        entry.Property(nameof(BaseEntity.UpdatedAt)).CurrentValue = now;
                        entry.Property(nameof(BaseEntity.UpdatedBy)).CurrentValue = userId;

                        // If the entity is being soft-deleted, IsDeleted will be true.
                        // The MarkAsDeleted method on BaseEntity should have set DeletedAt and DeletedBy.
                        // We ensure DbContext respects these values if set by domain logic, or sets them if state is just 'Deleted'.
                        if (entry.Property(nameof(BaseEntity.IsDeleted)).CurrentValue is true &&
                            entry.Property(nameof(BaseEntity.DeletedAt)).CurrentValue == null)
                        {
                            entry.Property(nameof(BaseEntity.DeletedAt)).CurrentValue = now;
                            entry.Property(nameof(BaseEntity.DeletedBy)).CurrentValue = userId;
                        }
                        break;

                    // case EntityState.Deleted: // This case is likely unreachable if soft delete is consistently used
                        // Soft deletes are handled when State is Modified and IsDeleted is true.
                        // If an entity somehow reaches here with State == Deleted, it implies a hard delete
                        // not going through the BaseEntity.MarkAsDeleted() flow, which should be avoided.
                        // The AuditLog will capture the 'Deleted' action anyway.
                        // Removing this case to prevent "unreachable code" warnings if all deletes are soft.
                        // break;
                }
            }

            // جمع آوری اطلاعات برای لاگ حسابرسی قبل از ذخیره تغییرات اصلی
            var auditEntries = OnBeforeSaveChanges();

            // ذخیره تغییرات اصلی موجودیت‌ها
            var result = await base.SaveChangesAsync(cancellationToken);

            // ثبت لاگ‌های حسابرسی پس از ذخیره موفق تغییرات اصلی
            await OnAfterSaveChanges(auditEntries, cancellationToken);

            return result;
        }

        // متد AuditChanges() حذف شد چون منطق آن در SaveChangesAsync ادغام شد

        private List<AuditEntry> OnBeforeSaveChanges()
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditEntry>();

            foreach (var entry in ChangeTracker.Entries())
            {
                // فقط موجودیت‌هایی که تغییر کرده‌اند و AuditLog نیستند را لاگ کن
                if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var auditEntry = new AuditEntry(entry)
                {
                    TableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name, // Get table name if possible
                    UserId = _currentUserService.UserId, // اطمینان از اینکه UserId قابل null است اگر کاربر احراز هویت نشده باشد
                    Action = entry.State.ToString()
                };
                auditEntries.Add(auditEntry);

                foreach (var property in entry.Properties)
                {
                    string propertyName = property.Metadata.Name;

                    if (property.IsTemporary)
                    {
                        // اگر پراپرتی موقتی است (مثلاً توسط دیتابیس تولید می‌شود)، آن را برای به‌روزرسانی بعد از ذخیره نگه دار
                        auditEntry.TemporaryProperties.Add(property);
                        continue;
                    }

                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue;
                        continue;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                            break;

                        case EntityState.Deleted:
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            break;

                        case EntityState.Modified:
                            if (property.IsModified) //  && (property.OriginalValue != null && !property.OriginalValue.Equals(property.CurrentValue)) || (property.CurrentValue != null && !property.CurrentValue.Equals(property.OriginalValue)))
                            {
                                // بررسی دقیق‌تر برای جلوگیری از ثبت مقادیر یکسان
                                if (property.OriginalValue == null && property.CurrentValue == null) continue;
                                if (property.OriginalValue != null && property.OriginalValue.Equals(property.CurrentValue)) continue;
                                if (property.CurrentValue != null && property.CurrentValue.Equals(property.OriginalValue)) continue;

                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }
            }
            return auditEntries;
        }

        private async Task OnAfterSaveChanges(List<AuditEntry> auditEntries, CancellationToken cancellationToken = default)
        {
            if (auditEntries == null || !auditEntries.Any())
                return;

            foreach (var auditEntry in auditEntries)
            {
                // به‌روزرسانی مقادیر پراپرتی‌های موقتی (مانند کلیدهای اصلی که توسط دیتابیس تولید شده‌اند)
                foreach (var prop in auditEntry.TemporaryProperties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                    else
                    {
                        auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                }
                AuditLogs.Add(auditEntry.ToAudit());
            }

            // ذخیره موجودیت‌های AuditLog بدون فعال کردن مجدد منطق حسابرسی برای خودشان
            // این اطمینان می‌دهد که فقط تغییرات AuditLog ذخیره می‌شوند و حلقه بازگشتی ایجاد نمی‌شود.
            if (AuditLogs.Local.Any()) // فقط اگر AuditLog جدیدی اضافه شده باشد
            {
                 await base.SaveChangesAsync(cancellationToken);
            }
        }
    }

    /// <summary>
    /// کلاس کمکی برای ثبت تغییرات
    /// </summary>
    public class AuditEntry
    {
        public AuditEntry(EntityEntry entry)
        {
            Entry = entry;
        }

        public EntityEntry Entry { get; }
        public string TableName { get; set; }
        public string UserId { get; set; }
        public string Action { get; set; }
        public Dictionary<string, object> KeyValues { get; } = new Dictionary<string, object>();
        public Dictionary<string, object> OldValues { get; } = new Dictionary<string, object>();
        public Dictionary<string, object> NewValues { get; } = new Dictionary<string, object>();
        public List<PropertyEntry> TemporaryProperties { get; } = new List<PropertyEntry>();

        public AuditLog ToAudit()
        {
            var audit = new AuditLog
            {
                TableName = TableName,
                UserId = UserId,
                Action = Action,
                KeyValues = JsonSerializer.Serialize(KeyValues),
                OldValues = OldValues.Count == 0 ? null : JsonSerializer.Serialize(OldValues),
                NewValues = NewValues.Count == 0 ? null : JsonSerializer.Serialize(NewValues),
                Timestamp = DateTime.UtcNow
            };

            return audit;
        }
    }
}