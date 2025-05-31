using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Application.DTOs;
using Authorization_Login_Asp.Net.Domain.Entities;

namespace Authorization_Login_Asp.Net.Application.Interfaces
{
    /// <summary>
    /// رابط دسترسی به داده‌های اعلان‌ها
    /// </summary>
    public interface INotificationRepository
    {
        /// <summary>
        /// افزودن اعلان جدید
        /// </summary>
        Task<Notification> AddAsync(Notification notification);

        /// <summary>
        /// به‌روزرسانی اعلان
        /// </summary>
        Task UpdateAsync(Notification notification);

        /// <summary>
        /// حذف اعلان
        /// </summary>
        Task DeleteAsync(Notification notification);

        /// <summary>
        /// دریافت اعلان با شناسه
        /// </summary>
        Task<Notification?> GetByIdAsync(Guid id);

        /// <summary>
        /// دریافت لیست اعلان‌ها با فیلتر
        /// </summary>
        Task<IEnumerable<Notification>> GetNotificationsAsync(NotificationFilter filter);

        /// <summary>
        /// دریافت اعلان‌های منقضی شده
        /// </summary>
        Task<IEnumerable<Notification>> GetExpiredNotificationsAsync();
    }
} 