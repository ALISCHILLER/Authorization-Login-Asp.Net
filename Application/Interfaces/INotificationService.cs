using System.Collections.Generic;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Application.DTOs;

namespace Authorization_Login_Asp.Net.Application.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationResponse>> GetNotificationsAsync(int count = 10);
        Task<NotificationResponse> CreateNotificationAsync(NotificationRequest request);
        Task MarkAsReadAsync(string id);
        Task DeleteNotificationAsync(string id);
    }
} 