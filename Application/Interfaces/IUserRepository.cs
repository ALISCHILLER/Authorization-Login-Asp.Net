using Authorization_Login_Asp.Net.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Application.Interfaces
{
    /// <summary>
    /// اینترفیس مخزن (Repository) مخصوص کاربر برای انجام عملیات تخصصی روی مدل User
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// دریافت همه کاربران به صورت غیرهمزمان
        /// </summary>
        /// <returns>لیستی از همه کاربران</returns>
        Task<IEnumerable<User>> GetAllAsync();

        /// <summary>
        /// دریافت کاربر بر اساس شناسه (Id)
        /// </summary>
        /// <param name="id">شناسه کاربر</param>
        /// <returns>شیء کاربر یا null در صورت عدم وجود</returns>
        Task<User> GetByIdAsync(Guid id);

        /// <summary>
        /// دریافت کاربر بر اساس ایمیل (برای فرایند ورود و اعتبارسنجی)
        /// </summary>
        /// <param name="email">آدرس ایمیل کاربر</param>
        /// <returns>شیء کاربر یا null در صورت عدم وجود</returns>
        Task<User> GetByEmailAsync(string email);

        /// <summary>
        /// دریافت کاربر بر اساس نام کاربری
        /// </summary>
        /// <param name="username">نام کاربری یکتا</param>
        /// <returns>شیء کاربر یا null در صورت عدم وجود</returns>
        Task<User> GetByUsernameAsync(string username);

        /// <summary>
        /// افزودن کاربر جدید به مخزن
        /// </summary>
        /// <param name="user">شیء کاربر برای افزودن</param>
        /// <returns>تسک غیرهمزمان</returns>
        Task AddAsync(User user);

        /// <summary>
        /// بروزرسانی اطلاعات یک کاربر موجود
        /// </summary>
        /// <param name="user">شیء کاربری که باید بروزرسانی شود</param>
        void Update(User user);

        /// <summary>
        /// حذف یک کاربر از مخزن
        /// </summary>
        /// <param name="user">شیء کاربری که باید حذف شود</param>
        void Remove(User user);

        /// <summary>
        /// ذخیره تغییرات انجام شده در مخزن به صورت غیرهمزمان
        /// </summary>
        /// <returns>تعداد رکوردهای تغییر یافته در دیتابیس</returns>
        Task<int> SaveChangesAsync();
    }
}
