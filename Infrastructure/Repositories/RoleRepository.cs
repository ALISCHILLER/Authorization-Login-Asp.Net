using Authorization_Login_Asp.Net.Application.Interfaces;
using Authorization_Login_Asp.Net.Domain.Entities;
using Authorization_Login_Asp.Net.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Infrastructure.Repositories
{
    /// <summary>
    /// پیاده‌سازی مخزن (Repository) نقش‌ها با استفاده از Entity Framework Core
    /// </summary>
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Role> _roles;

        /// <summary>
        /// سازنده با تزریق DbContext
        /// </summary>
        /// <param name="context">شیء DbContext پروژه</param>
        public RoleRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _roles = _context.Set<Role>();
        }

        /// <summary>
        /// دریافت تمام نقش‌ها به صورت لیست
        /// </summary>
        /// <returns>لیست نقش‌ها</returns>
        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await _roles
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .ToListAsync();
        }

        /// <summary>
        /// دریافت یک نقش بر اساس آی‌دی
        /// </summary>
        /// <param name="id">آی‌دی نقش</param>
        /// <returns>نقش یا null اگر یافت نشد</returns>
        public async Task<Role> GetByIdAsync(Guid id)
        {
            return await _roles
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        /// <summary>
        /// افزودن یک نقش جدید
        /// </summary>
        /// <param name="role">شیء نقش</param>
        /// <returns>کار انجام شد یا خیر</returns>
        public async Task AddAsync(Role role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            await _roles.AddAsync(role);
        }

        /// <summary>
        /// حذف نقش
        /// </summary>
        /// <param name="role">شیء نقش</param>
        public void Remove(Role role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            _roles.Remove(role);
        }

        /// <summary>
        /// بروزرسانی اطلاعات نقش
        /// </summary>
        /// <param name="role">شیء نقش</param>
        public void Update(Role role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            _context.Entry(role).State = EntityState.Modified;
        }

        /// <summary>
        /// بررسی وجود نقش بر اساس نام
        /// </summary>
        /// <param name="name">نام نقش</param>
        /// <returns>True اگر وجود داشته باشد</returns>
        public async Task<bool> ExistsByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("نام نقش نمی‌تواند خالی باشد.", nameof(name));

            return await _roles.AnyAsync(r => r.Name == name);
        }
    }
}