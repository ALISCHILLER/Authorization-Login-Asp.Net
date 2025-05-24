using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Domain.Entities;

namespace Authorization_Login_Asp.Net.Application.Interfaces
{
    /// <summary>
    /// اینترفیس ریپازیتوری برای عملیات مرتبط با نقش‌ها (Role)
    /// </summary>
    public interface IRoleRepository
    {
        /// <summary>
        /// دریافت تمام نقش‌ها به صورت لیست
        /// </summary>
        /// <returns>لیست نقش‌ها</returns>
        Task<IEnumerable<Role>> GetAllAsync();

        /// <summary>
        /// دریافت یک نقش بر اساس آی‌دی
        /// </summary>
        /// <param name="id">آی‌دی نقش</param>
        /// <returns>نقش یا null اگر یافت نشد</returns>
        Task<Role> GetByIdAsync(Guid id);

        /// <summary>
        /// افزودن یک نقش جدید
        /// </summary>
        /// <param name="role">شیء نقش</param>
        /// <returns>کار انجام شد یا خیر</returns>
        Task AddAsync(Role role);

        /// <summary>
        /// حذف نقش
        /// </summary>
        /// <param name="role">شیء نقش</param>
        void Remove(Role role);

        /// <summary>
        /// بروزرسانی اطلاعات نقش (اگر نیاز به متد جداگانه بود)
        /// </summary>
        /// <param name="role">شیء نقش</param>
        void Update(Role role);

        /// <summary>
        /// بررسی وجود نقش بر اساس نام (جهت جلوگیری از تکرار)
        /// </summary>
        /// <param name="name">نام نقش</param>
        /// <returns>True اگر وجود داشته باشد</returns>
        Task<bool> ExistsByNameAsync(string name);
    }
}
