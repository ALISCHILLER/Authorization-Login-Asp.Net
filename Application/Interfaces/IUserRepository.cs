using Authorization_Login_Asp.Net.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Application.Interfaces
{
    /// <summary>
    /// اینترفیس مخزن (Repository) مخصوص کاربر برای انجام عملیات تخصصی روی مدل User
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// دریافت همه کاربران به صورت غیرهمزمان و با امکان لغو عملیات
        /// </summary>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>لیستی از همه کاربران</returns>
        Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت کاربر بر اساس شناسه (Id) به صورت غیرهمزمان و با امکان لغو عملیات
        /// </summary>
        /// <param name="id">شناسه کاربر</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>شیء کاربر یا null در صورت عدم وجود</returns>
        Task<User> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت کاربر بر اساس ایمیل (برای فرایند ورود و اعتبارسنجی) به صورت غیرهمزمان و با امکان لغو عملیات
        /// </summary>
        /// <param name="email">آدرس ایمیل کاربر</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>شیء کاربر یا null در صورت عدم وجود</returns>
        Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت کاربر بر اساس نام کاربری به صورت غیرهمزمان و با امکان لغو عملیات
        /// </summary>
        /// <param name="username">نام کاربری یکتا</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>شیء کاربر یا null در صورت عدم وجود</returns>
        Task<User> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

        /// <summary>
        /// افزودن کاربر جدید به مخزن به صورت غیرهمزمان و با امکان لغو عملیات
        /// </summary>
        /// <param name="user">شیء کاربر برای افزودن</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>تسک غیرهمزمان</returns>
        Task AddAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// بروزرسانی اطلاعات یک کاربر موجود در کانتکست (تا ذخیره تغییرات)
        /// </summary>
        /// <param name="user">شیء کاربری که باید بروزرسانی شود</param>
        void Update(User user);

        /// <summary>
        /// حذف یک کاربر از کانتکست (تا ذخیره تغییرات)
        /// </summary>
        /// <param name="user">شیء کاربری که باید حذف شود</param>
        void Remove(User user);

        /// <summary>
        /// ذخیره تغییرات انجام شده در مخزن به صورت غیرهمزمان و با امکان لغو عملیات
        /// </summary>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>تعداد رکوردهای تغییر یافته در دیتابیس</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// بررسی وجود کاربر با ایمیل مشخص به صورت غیرهمزمان و با امکان لغو عملیات
        /// </summary>
        /// <param name="email">آدرس ایمیل کاربر</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>نتیجه بررسی وجود کاربر</returns>
        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// بررسی وجود کاربر با نام کاربری مشخص به صورت غیرهمزمان و با امکان لغو عملیات
        /// </summary>
        /// <param name="username">نام کاربری</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>نتیجه بررسی وجود کاربر</returns>
        Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default);

        /// <summary>
        /// حذف یک کاربر به صورت غیرهمزمان و با امکان لغو عملیات
        /// </summary>
        /// <param name="user">شیء کاربری که باید حذف شود</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>تسک غیرهمزمان</returns>
        Task DeleteAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت تاریخچه ورودهای کاربر به صورت صفحه‌بندی شده
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="page">شماره صفحه</param>
        /// <param name="pageSize">تعداد آیتم در هر صفحه</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>لیست تاریخچه ورودها</returns>
        Task<IEnumerable<LoginHistory>> GetLoginHistoryAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت تعداد کل رکوردهای تاریخچه ورود کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>تعداد کل رکوردها</returns>
        Task<int> GetLoginHistoryCountAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
