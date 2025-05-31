using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Domain.Entities;

namespace Authorization_Login_Asp.Net.Domain.Interfaces
{
    /// <summary>
    /// رابط مخزن اعلان‌ها
    /// </summary>
    public interface INotificationRepository
    {
        /// <summary>
        /// دریافت اعلان‌های کاربر با فیلتر
        /// </summary>
        Task<(IEnumerable<Notification> Notifications, int TotalCount)> GetUserNotificationsAsync(
            Guid userId,
            NotificationType? type = null,
            AlertSeverity? severity = null,
            bool? isRead = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت اعلان با شناسه
        /// </summary>
        Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// افزودن اعلان جدید
        /// </summary>
        Task<Notification> AddAsync(Notification notification, CancellationToken cancellationToken = default);

        /// <summary>
        /// به‌روزرسانی اعلان
        /// </summary>
        Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default);

        /// <summary>
        /// حذف اعلان
        /// </summary>
        Task DeleteAsync(Notification notification, CancellationToken cancellationToken = default);

        /// <summary>
        /// حذف اعلان‌های منقضی شده
        /// </summary>
        Task<int> DeleteExpiredNotificationsAsync(CancellationToken cancellationToken = default);
    }
} 