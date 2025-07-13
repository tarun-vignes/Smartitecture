using System;

namespace Smartitecture.Core.Events
{
    public interface IEvent
    {
        Guid Id { get; }
        DateTime OccurredOn { get; }
        string EventType { get; }
        string CorrelationId { get; }
    }

    public abstract class Event : IEvent
    {
        public Guid Id { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
        public string EventType => GetType().Name;
        public string CorrelationId { get; set; } = string.Empty;
    }

    public interface IEventHandler<TEvent> where TEvent : IEvent
    {
        Task HandleAsync(TEvent @event);
    }
}
