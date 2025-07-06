using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Authorization_Login_Asp.Net.Core.Domain.Events
{
    /// <summary>
    /// کلاس مدیریت و ارسال رویدادهای دامنه
    /// </summary>
    public class EventDispatcher : IEventDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EventDispatcher> _logger;

        public EventDispatcher(IServiceProvider serviceProvider, ILogger<EventDispatcher> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task DispatchAsync<TEvent>(TEvent @event) where TEvent : IDomainEvent
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var handlers = scope.ServiceProvider.GetServices<IEventHandler<TEvent>>().ToList();

                if (!handlers.Any())
                {
                    _logger.LogWarning("No handlers found for event type {EventType}", typeof(TEvent).Name);
                    return;
                }

                _logger.LogInformation("Dispatching event {EventType} with ID {EventId}", @event.EventType, @event.Id);

                var tasks = handlers.Select(async handler =>
                {
                    try
                    {
                        await handler.HandleAsync(@event);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error handling event {EventType} with ID {EventId} in handler {HandlerType}",
                            @event.EventType, @event.Id, handler.GetType().Name);
                        throw;
                    }
                });

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dispatching event {EventType} with ID {EventId}",
                    @event.EventType, @event.Id);
                throw;
            }
        }

        public async Task DispatchAsync(IEnumerable<IDomainEvent> events)
        {
            var eventsList = events.ToList();
            if (!eventsList.Any())
            {
                _logger.LogInformation("No events to dispatch");
                return;
            }

            _logger.LogInformation("Dispatching {Count} events", eventsList.Count);

            var tasks = eventsList.Select(async @event =>
            {
                try
                {
                    await DispatchAsync(@event);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error dispatching event {EventType} with ID {EventId}",
                        @event.EventType, @event.Id);
                    throw;
                }
            });

            await Task.WhenAll(tasks);
        }

        private async Task DispatchAsync(IDomainEvent @event)
        {
            var eventType = @event.GetType();
            var dispatchMethod = typeof(EventDispatcher)
                .GetMethod(nameof(DispatchAsync))
                .MakeGenericMethod(eventType);

            await (Task)dispatchMethod.Invoke(this, new object[] { @event });
        }
    }
}