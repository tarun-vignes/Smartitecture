using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Smartitecture.Core.Events
{
    public interface IEventBus
    {
        Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent;
        void Subscribe<TEvent, TEventHandler>()
            where TEvent : IEvent
            where TEventHandler : IEventHandler<TEvent>;
        void Unsubscribe<TEvent, TEventHandler>()
            where TEvent : IEvent
            where TEventHandler : IEventHandler<TEvent>;
    }

    public class EventBus : IEventBus
    {
        private readonly ILogger<EventBus> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<string, List<Type>> _handlers;

        public EventBus(ILogger<EventBus> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _handlers = new ConcurrentDictionary<string, List<Type>>();
        }

        public void Subscribe<TEvent, TEventHandler>()
            where TEvent : IEvent
            where TEventHandler : IEventHandler<TEvent>
        {
            var eventName = typeof(TEvent).Name;
            var handlerType = typeof(TEventHandler);

            if (!_handlers.ContainsKey(eventName))
            {
                _handlers.TryAdd(eventName, new List<Type> { handlerType });
            }
            else
            {
                var handlers = _handlers[eventName];
                if (!handlers.Contains(handlerType))
                {
                    handlers.Add(handlerType);
                }
            }

            _logger.LogInformation("Subscribed event handler {Handler} for event {Event}", 
                handlerType.Name, eventName);
        }

        public void Unsubscribe<TEvent, TEventHandler>()
            where TEvent : IEvent
            where TEventHandler : IEventHandler<TEvent>
        {
            var eventName = typeof(TEvent).Name;
            var handlerType = typeof(TEventHandler);

            if (_handlers.TryGetValue(eventName, out var handlers))
            {
                handlers.Remove(handlerType);
                if (handlers.Count == 0)
                {
                    _handlers.TryRemove(eventName, out _);
                }
            }

            _logger.LogInformation("Unsubscribed event handler {Handler} for event {Event}", 
                handlerType.Name, eventName);
        }

        public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent
        {
            var eventName = typeof(TEvent).Name;
            var handlers = _handlers.GetValueOrDefault(eventName);

            if (handlers?.Any() != true) return;

            _logger.LogInformation("Publishing event {Event} with ID {Id}", 
                eventName, @event.Id);

            foreach (var handlerType in handlers)
            {
                try
                {
                    var handler = _serviceProvider.GetRequiredService(handlerType)
                        as IEventHandler<TEvent>;

                    if (handler != null)
                    {
                        await handler.HandleAsync(@event);
                        _logger.LogInformation("Successfully handled event {Event} by {Handler}", 
                            eventName, handlerType.Name);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling event {Event} by {Handler}", 
                        eventName, handlerType.Name);
                }
            }
        }
    }
}
