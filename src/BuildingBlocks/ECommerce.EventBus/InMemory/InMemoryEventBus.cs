using ECommerce.EventBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ECommerce.EventBus.InMemory;

public class InMemoryEventBus : IEventBus
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InMemoryEventBus> _logger;
    private readonly Dictionary<Type, List<Type>> _handlers = new();

    public InMemoryEventBus(IServiceProvider serviceProvider, ILogger<InMemoryEventBus> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public void Subscribe<TEvent, THandler>()
        where TEvent : IntegrationEvent
        where THandler : IEventHandler<TEvent>
    {
        var eventType = typeof(TEvent);
        var handlerType = typeof(THandler);

        if (!_handlers.ContainsKey(eventType))
        {
            _handlers[eventType] = new List<Type>();
        }

        if (!_handlers[eventType].Contains(handlerType))
        {
            _handlers[eventType].Add(handlerType);
            _logger.LogInformation("Subscribed {Handler} to {Event}", handlerType.Name, eventType.Name);
        }
    }

    public async Task PublishAsync<T>(T @event) where T : IntegrationEvent
    {
        var eventType = @event.GetType();
        _logger.LogInformation("Publishing event {EventType} with ID {EventId}", eventType.Name, @event.Id);

        if (_handlers.TryGetValue(eventType, out var handlerTypes))
        {
            using var scope = _serviceProvider.CreateScope();
            
            foreach (var handlerType in handlerTypes)
            {
                try
                {
                    var handler = scope.ServiceProvider.GetService(handlerType);
                    if (handler == null)
                    {
                        _logger.LogWarning("Handler {HandlerType} not registered in DI", handlerType.Name);
                        continue;
                    }

                    var handleMethod = handlerType.GetMethod("HandleAsync");
                    if (handleMethod != null)
                    {
                        await (Task)handleMethod.Invoke(handler, new object[] { @event })!;
                        _logger.LogInformation("Event {EventType} handled by {Handler}", eventType.Name, handlerType.Name);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling event {EventType} with handler {Handler}", 
                        eventType.Name, handlerType.Name);
                }
            }
        }
        else
        {
            _logger.LogWarning("No handlers registered for event {EventType}", eventType.Name);
        }
    }
}
