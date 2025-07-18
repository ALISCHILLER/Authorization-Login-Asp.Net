using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Domain.Entities;

namespace Authorization_Login_Asp.Net.Core.Domain.Interfaces
{
    public interface IUserNotificationRepository : IGenericRepository<UserNotification>
    {
        Task<IEnumerable<UserNotification>> GetByUserAsync(Guid userId, bool includeRead = false, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserNotification>> GetByTypeAsync(string notificationType, bool includeRead = false, CancellationToken cancellationToken = default);
        Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<bool> MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
        Task<bool> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<UserNotification> AddNotificationAsync(Guid userId, string title, string message, string notificationType, string data = null, CancellationToken cancellationToken = default);
        Task<bool> DeleteNotificationAsync(Guid notificationId, CancellationToken cancellationToken = default);
        Task<int> CleanupOldNotificationsAsync(TimeSpan age, CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> GetDistinctNotificationTypesAsync(CancellationToken cancellationToken = default);
        Task<IDictionary<string, int>> GetNotificationTypeStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    }
}
