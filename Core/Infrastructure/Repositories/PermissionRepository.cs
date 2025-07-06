using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Domain.Interfaces;
using Authorization_Login_Asp.Net.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Repositories
{
    /// <summary>
    /// پیاده‌سازی مخزن دسترسی‌ها
    /// این کلاس عملیات مربوط به دسترسی‌ها را در پایگاه داده پیاده‌سازی می‌کند
    /// </summary>
    public class PermissionRepository : IPermissionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Permission> _dbSet;

        /// <summary>
        /// سازنده کلاس مخزن دسترسی‌ها
        /// </summary>
        /// <param name="context">کانتکست پایگاه داده</param>
        public PermissionRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<Permission>();
        }

        /// <summary>
        /// دریافت دسترسی با شناسه
        /// </summary>
        /// <param name="id">شناسه دسترسی</param>
        /// <returns>دسترسی مورد نظر در صورت وجود</returns>
        public async Task<Permission?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <summary>
        /// دریافت دسترسی با نام
        /// </summary>
        /// <param name="name">نام دسترسی</param>
        /// <returns>دسترسی مورد نظر در صورت وجود</returns>
        public async Task<Permission?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.Name == name && !p.IsDeleted);
        }

        /// <summary>
        /// دریافت دسترسی‌های یک نقش
        /// </summary>
        /// <param name="roleId">شناسه نقش</param>
        /// <returns>لیست دسترسی‌های نقش مورد نظر</returns>
        public async Task<IEnumerable<Permission>> GetByRoleAsync(Guid roleId)
        {
            return await _dbSet.Where(p => p.Roles.Any(r => r.Id == roleId) && !p.IsDeleted).ToListAsync();
        }

        /// <summary>
        /// دریافت دسترسی‌های یک نقش با نام
        /// </summary>
        /// <param name="roleName">نام نقش</param>
        /// <returns>لیست دسترسی‌های نقش مورد نظر</returns>
        public async Task<IEnumerable<Permission>> GetByRoleNameAsync(string roleName)
        {
            return await _dbSet.Where(p => p.Roles.Any(r => r.Name == roleName) && !p.IsDeleted).ToListAsync();
        }

        /// <summary>
        /// دریافت دسترسی‌های یک کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>لیست دسترسی‌های کاربر مورد نظر</returns>
        public async Task<IEnumerable<Permission>> GetByUserAsync(Guid userId)
        {
            return await _dbSet.Where(p => p.Roles.Any(r => r.Users.Any(u => u.Id == userId)) && !p.IsDeleted).ToListAsync();
        }

        /// <summary>
        /// دریافت دسترسی‌های یک کاربر با نام کاربری
        /// </summary>
        /// <param name="username">نام کاربری</param>
        /// <returns>لیست دسترسی‌های کاربر مورد نظر</returns>
        public async Task<IEnumerable<Permission>> GetByUsernameAsync(string username)
        {
            return await _dbSet.Where(p => p.Roles.Any(r => r.Users.Any(u => u.Username == username)) && !p.IsDeleted).ToListAsync();
        }

        // IGenericRepository methods (نمونه ساده)
        /// <summary>
        /// دریافت تمام دسترسی‌ها
        /// </summary>
        /// <returns>لیست تمام دسترسی‌ها</returns>
        public async Task<IEnumerable<Permission>> GetAllAsync()
        {
            return await _dbSet.Where(p => !p.IsDeleted).ToListAsync();
        }

        /// <summary>
        /// افزودن یک دسترسی جدید
        /// </summary>
        /// <param name="entity">دسترسی مورد نظر</param>
        public async Task AddAsync(Permission entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// به‌روزرسانی یک دسترسی
        /// </summary>
        /// <param name="entity">دسترسی مورد نظر</param>
        public async Task UpdateAsync(Permission entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// حذف یک دسترسی
        /// </summary>
        /// <param name="entity">دسترسی مورد نظر</param>
        public async Task RemoveAsync(Permission entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// بررسی وجود دسترسی با شناسه
        /// </summary>
        /// <param name="id">شناسه دسترسی</param>
        /// <returns>درست اگر دسترسی وجود داشته باشد</returns>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _dbSet.AnyAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<IEnumerable<Permission>> FindAsync(System.Linq.Expressions.Expression<System.Func<Permission, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public void Update(Permission entity)
        {
            _dbSet.Update(entity);
        }

        public void Remove(Permission entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task<bool> ExistsAsync(System.Linq.Expressions.Expression<System.Func<Permission, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public async Task<int> CountAsync(System.Linq.Expressions.Expression<System.Func<Permission, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}