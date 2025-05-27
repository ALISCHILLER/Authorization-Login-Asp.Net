using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Domain.Events
{
    /// <summary>
    /// رابط مدیریت‌کننده رویدادهای دامنه
    /// </summary>
    public interface IEventHandler<in TEvent> where TEvent : IDomainEvent
    {
        /// <summary>
        /// پردازش رویداد
        /// </summary>
        Task HandleAsync(TEvent @event);
    }
} 