using ECommerce.EventBus.Abstractions;

namespace ECommerce.EventBus.InMemory;

public class InMemoryEventBus : IEventBus
{
    private readonly Dictionary<Type, List<Func<IntegrationEvent, Task>>> _handlers = new();

    public void Subscribe<TEvent, THandler>()
        where TEvent : IntegrationEvent
        where THandler : class
    {
        var eventType = typeof(TEvent);
        if (!_handlers.ContainsKey(eventType))
        {
            _handlers[eventType] = new List<Func<IntegrationEvent, Task>>();
        }
    }

    public Task PublishAsync<T>(T @event) where T : IntegrationEvent
    {
        var eventType = @event.GetType();
        if (_handlers.ContainsKey(eventType))
        {
            var handlers = _handlers[eventType];
            foreach (var handler in handlers)
            {
                _ = Task.Run(() => handler(@event));
            }
        }
        return Task.CompletedTask;
    }

    public void Publish<T>(T @event) where T : IntegrationEvent
    {
        _ = PublishAsync(@event);
    }
}
