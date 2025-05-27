using System.Collections.Generic;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Domain.Events
{
    /// <summary>
    /// رابط مدیریت و ارسال رویدادهای دامنه
    /// </summary>
    public interface IEventDispatcher
    {
        /// <summary>
        /// ارسال یک رویداد
        /// </summary>
        Task DispatchAsync<TEvent>(TEvent @event) where TEvent : IDomainEvent;

        /// <summary>
        /// ارسال مجموعه‌ای از رویدادها
        /// </summary>
        Task DispatchAsync(IEnumerable<IDomainEvent> events);
    }
} 