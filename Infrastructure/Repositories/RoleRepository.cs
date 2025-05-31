// استفاده از رابط‌های تعریف شده در لایه کاربرد
using Authorization_Login_Asp.Net.Application.Interfaces;
// استفاده از موجودیت‌های تعریف شده در لایه دامنه
using Authorization_Login_Asp.Net.Domain.Entities;
// استفاده از شمارشی‌های تعریف شده در لایه دامنه
using Authorization_Login_Asp.Net.Domain.Enums;
// استفاده از کلاس‌های پایگاه داده در لایه زیرساخت
using Authorization_Login_Asp.Net.Infrastructure.Data;
// استفاده از Entity Framework Core برای عملیات پایگاه داده
using Microsoft.EntityFrameworkCore;
// استفاده از کلاس‌های پایه سیستم
using System;
// استفاده از کلاس‌های مجموعه‌ها
using System.Collections.Generic;
// استفاده از کلاس‌های LINQ
using System.Linq;
// استفاده از کلاس‌های مدیریت نخ‌ها
using System.Threading;
// استفاده از کلاس‌های عملیات ناهمگام
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Infrastructure.Repositories
{
    /// <summary>
    /// پیاده‌سازی مخزن نقش‌ها
    /// این کلاس عملیات مربوط به نقش‌ها را در پایگاه داده پیاده‌سازی می‌کند
    /// شامل عملیات‌های CRUD و جستجوی نقش‌ها، مدیریت دسترسی‌ها و بررسی نقش‌های کاربران
    /// </summary>
    public class RoleRepository : Repository<Role>, IRoleRepository
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// سازنده کلاس مخزن نقش‌ها
        /// </summary>
        /// <param name="context">کانتکست پایگاه داده</param>
        public RoleRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// دریافت تمام نقش‌ها به صورت لیست
        /// </summary>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>لیست نقش‌ها</returns>
        public async Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(r => r.Permissions)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// دریافت یک نقش بر اساس آی‌دی
        /// </summary>
        /// <param name="id">آی‌دی نقش</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>نقش یا null اگر یافت نشد</returns>
        public async Task<Role> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        }

        /// <summary>
        /// دریافت نقش با نام مشخص
        /// </summary>
        /// <param name="name">نام نقش</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>نقش مورد نظر در صورت وجود</returns>
        public async Task<Role> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("نام نقش نمی‌تواند خالی باشد", nameof(name));

            return await _dbSet
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
        }

        /// <summary>
        /// افزودن یک نقش جدید
        /// </summary>
        /// <param name="role">شیء نقش</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>تسک غیرهمزمان</returns>
        public async Task AddAsync(Role role, CancellationToken cancellationToken = default)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            await _dbSet.AddAsync(role, cancellationToken);
            await SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// حذف نقش
        /// </summary>
        /// <param name="role">شیء نقش</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>تسک غیرهمزمان</returns>
        public async Task DeleteAsync(Role role, CancellationToken cancellationToken = default)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            _dbSet.Remove(role);
            await SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// بروزرسانی اطلاعات نقش
        /// </summary>
        /// <param name="role">شیء نقش</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>تسک غیرهمزمان</returns>
        public async Task UpdateAsync(Role role, CancellationToken cancellationToken = default)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            _context.Entry(role).State = EntityState.Modified;
            await SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// حذف نقش بر اساس آی‌دی
        /// </summary>
        /// <param name="id">آی‌دی نقش</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>تسک غیرهمزمان</returns>
        public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var role = await GetByIdAsync(id, cancellationToken);
            if (role != null)
            {
                await DeleteAsync(role, cancellationToken);
            }
        }

        /// <summary>
        /// بررسی وجود نقش بر اساس نام
        /// </summary>
        /// <param name="name">نام نقش</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>درست اگر نقش وجود داشته باشد</returns>
        public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("نام نقش نمی‌تواند خالی باشد", nameof(name));

            return await _dbSet.AnyAsync(r => r.Name == name, cancellationToken);
        }

        /// <summary>
        /// ذخیره تغییرات
        /// </summary>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>تعداد رکوردهای تغییر یافته</returns>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                foreach (var entry in _context.ChangeTracker.Entries())
                {
                    entry.State = EntityState.Detached;
                }
                throw;
            }
            catch (DbUpdateException)
            {
                foreach (var entry in _context.ChangeTracker.Entries())
                {
                    entry.State = EntityState.Detached;
                }
                throw;
            }
        }

        /// <summary>
        /// دریافت نقش‌های یک نوع خاص
        /// </summary>
        /// <param name="type">نوع نقش</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست نقش‌های نوع مورد نظر</returns>
        public async Task<IEnumerable<Role>> GetByTypeAsync(RoleType type, CancellationToken cancellationToken = default)
        {
            var roleNames = type switch
            {
                RoleType.Admin => new[] { "Admin" },
                RoleType.User => new[] { "User" },
                RoleType.Operator => new[] { "Operator" },
                RoleType.ContentManager => new[] { "ContentManager" },
                RoleType.Support => new[] { "Support" },
                _ => Array.Empty<string>()
            };

            return await _dbSet
                .Include(r => r.Permissions)
                .Where(r => roleNames.Contains(r.Name))
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// دریافت نقش‌های فعال
        /// </summary>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست نقش‌های فعال</returns>
        public async Task<IEnumerable<Role>> GetActiveAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(r => r.Permissions)
                .Where(r => r.IsActive)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// دریافت نقش‌های یک کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست نقش‌های کاربر مورد نظر</returns>
        public async Task<IEnumerable<Role>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(r => r.Permissions)
                .Where(r => r.Users.Any(u => u.Id == userId))
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// بررسی وجود نقش در یک نوع خاص
        /// </summary>
        /// <param name="name">نام نقش</param>
        /// <param name="type">نوع نقش</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر نقش در نوع مورد نظر وجود داشته باشد</returns>
        public async Task<bool> ExistsInTypeAsync(string name, RoleType type, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("نام نقش نمی‌تواند خالی باشد", nameof(name));

            var roleNames = type switch
            {
                RoleType.Admin => new[] { "Admin" },
                RoleType.User => new[] { "User" },
                RoleType.Operator => new[] { "Operator" },
                RoleType.ContentManager => new[] { "ContentManager" },
                RoleType.Support => new[] { "Support" },
                _ => Array.Empty<string>()
            };

            return await _dbSet.AnyAsync(r => r.Name == name && roleNames.Contains(r.Name), cancellationToken);
        }

        /// <summary>
        /// دریافت نقش‌های سیستمی
        /// </summary>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست نقش‌های سیستمی</returns>
        public async Task<IEnumerable<Role>> GetSystemRolesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(r => r.Permissions)
                .Where(r => r.Name == "Admin" || r.Name == "Operator")
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// دریافت نقش‌های کاربری
        /// </summary>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست نقش‌های کاربری</returns>
        public async Task<IEnumerable<Role>> GetUserRolesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(r => r.Permissions)
                .Where(r => r.Name == "User" || r.Name == "ContentManager" || r.Name == "Support")
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// دریافت نقش‌های یک کاربر به همراه دسترسی‌های آن‌ها
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست نقش‌های کاربر با دسترسی‌های آن‌ها</returns>
        public async Task<IEnumerable<Role>> GetUserRolesWithPermissionsAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("شناسه کاربر نمی‌تواند خالی باشد", nameof(userId));

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new ArgumentException("شناسه کاربر باید یک GUID معتبر باشد", nameof(userId));

            return await _dbSet
                .Include(r => r.Permissions)
                .Where(r => r.Users.Any(u => u.Id == userGuid))
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// بررسی تعلق کاربر به یک نقش
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="roleName">نام نقش</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به نقش تعلق داشته باشد</returns>
        public async Task<bool> IsUserInRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("نام نقش نمی‌تواند خالی باشد", nameof(roleName));

            return await _dbSet
                .AnyAsync(r => r.Name == roleName && r.Users.Any(u => u.Id == userId), cancellationToken);
        }

        /// <summary>
        /// بررسی تعلق کاربر به چند نقش
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="roleNames">نام‌های نقش‌ها</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به همه نقش‌ها تعلق داشته باشد</returns>
        public async Task<bool> IsUserInAllRolesAsync(Guid userId, IEnumerable<string> roleNames, CancellationToken cancellationToken = default)
        {
            if (roleNames == null || !roleNames.Any())
                throw new ArgumentException("لیست نقش‌ها نمی‌تواند خالی باشد", nameof(roleNames));

            var userRoles = await _dbSet
                .Where(r => r.Users.Any(u => u.Id == userId))
                .Select(r => r.Name)
                .ToListAsync(cancellationToken);

            return roleNames.All(roleName => userRoles.Contains(roleName));
        }

        /// <summary>
        /// بررسی تعلق کاربر به حداقل یکی از نقش‌ها
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="roleNames">نام‌های نقش‌ها</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از نقش‌ها تعلق داشته باشد</returns>
        public async Task<bool> IsUserInAnyRoleAsync(Guid userId, IEnumerable<string> roleNames, CancellationToken cancellationToken = default)
        {
            if (roleNames == null || !roleNames.Any())
                throw new ArgumentException("لیست نقش‌ها نمی‌تواند خالی باشد", nameof(roleNames));

            return await _dbSet
                .AnyAsync(r => roleNames.Contains(r.Name) && r.Users.Any(u => u.Id == userId), cancellationToken);
        }

        public async Task AddUserToRoleAsync(Guid userId, string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("نام نقش نمی‌تواند خالی باشد", nameof(roleName));

            var role = await GetByNameAsync(roleName);
            if (role == null)
                throw new ArgumentException($"نقش {roleName} یافت نشد", nameof(roleName));

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new ArgumentException($"کاربر با شناسه {userId} یافت نشد", nameof(userId));

            if (!await IsUserInRoleAsync(userId, roleName))
            {
                var userRole = new UserRole
                {
                    UserId = userId,
                    RoleId = role.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.UserRoles.AddAsync(userRole);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveUserFromRoleAsync(Guid userId, string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("نام نقش نمی‌تواند خالی باشد", nameof(roleName));

            var role = await GetByNameAsync(roleName);
            if (role == null)
                throw new ArgumentException($"نقش {roleName} یافت نشد", nameof(roleName));

            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);
            
            if (userRole != null)
            {
                _context.UserRoles.Remove(userRole);
                await _context.SaveChangesAsync();
            }
        }
    }
}