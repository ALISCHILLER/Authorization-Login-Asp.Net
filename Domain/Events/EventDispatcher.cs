using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Authorization_Login_Asp.Net.Domain.Events
{
    /// <summary>
    /// کلاس مدیریت و ارسال رویدادهای دامنه
    /// </summary>
    public class EventDispatcher : IEventDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public EventDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task DispatchAsync<TEvent>(TEvent @event) where TEvent : IDomainEvent
        {
            using var scope = _serviceProvider.CreateScope();
            var handlers = scope.ServiceProvider.GetServices<IEventHandler<TEvent>>();

            foreach (var handler in handlers)
            {
                await handler.HandleAsync(@event);
            }
        }

        public async Task DispatchAsync(IEnumerable<IDomainEvent> events)
        {
            foreach (var @event in events)
            {
                await DispatchAsync(@event);
            }
        }
    }
} 