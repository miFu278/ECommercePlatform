using System.Text;
using System.Text.Json;
using ECommerce.EventBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ECommerce.EventBus.RabbitMQ;

public class RabbitMQEventBus : IEventBus, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMQEventBus> _logger;
    private readonly string _connectionString;
    private readonly string _exchangeName;
    private readonly string _queueName;
    private readonly Dictionary<string, List<Type>> _handlers = new();
    private readonly Dictionary<string, Type> _eventTypes = new();
    
    private IConnection? _connection;
    private IModel? _channel;
    private bool _disposed;

    public RabbitMQEventBus(
        IServiceProvider serviceProvider,
        ILogger<RabbitMQEventBus> logger,
        string connectionString,
        string queueName = "ecommerce_events")
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _connectionString = connectionString;
        _exchangeName = "ecommerce_event_bus";
        _queueName = queueName;
        
        InitializeRabbitMQ();
    }

    private void InitializeRabbitMQ()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri(_connectionString),
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare exchange
            _channel.ExchangeDeclare(
                exchange: _exchangeName,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false);

            // Declare queue
            _channel.QueueDeclare(
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _logger.LogInformation("RabbitMQ connection established. Exchange: {Exchange}, Queue: {Queue}", 
                _exchangeName, _queueName);

            // Start consuming
            StartConsuming();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
            throw;
        }
    }

    private void StartConsuming()
    {
        if (_channel == null) return;

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (sender, args) =>
        {
            var eventName = args.RoutingKey;
            var message = Encoding.UTF8.GetString(args.Body.ToArray());

            try
            {
                await ProcessEvent(eventName, message);
                _channel?.BasicAck(args.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event {EventName}", eventName);
                // Nack and requeue on failure
                _channel?.BasicNack(args.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsume(
            queue: _queueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation("Started consuming from queue: {Queue}", _queueName);
    }

    public void Subscribe<TEvent, THandler>()
        where TEvent : IntegrationEvent
        where THandler : IEventHandler<TEvent>
    {
        var eventName = typeof(TEvent).Name;
        var handlerType = typeof(THandler);

        if (!_handlers.ContainsKey(eventName))
        {
            _handlers[eventName] = new List<Type>();
        }

        if (!_handlers[eventName].Contains(handlerType))
        {
            _handlers[eventName].Add(handlerType);
            _eventTypes[eventName] = typeof(TEvent);

            // Bind queue to exchange with routing key
            _channel?.QueueBind(
                queue: _queueName,
                exchange: _exchangeName,
                routingKey: eventName);

            _logger.LogInformation("Subscribed {Handler} to {Event}", handlerType.Name, eventName);
        }
    }

    public async Task PublishAsync<T>(T @event) where T : IntegrationEvent
    {
        if (_channel == null)
        {
            _logger.LogWarning("RabbitMQ channel is not available");
            return;
        }

        var eventName = @event.GetType().Name;
        var message = JsonSerializer.Serialize(@event, @event.GetType());
        var body = Encoding.UTF8.GetBytes(message);

        var properties = _channel.CreateBasicProperties();
        properties.DeliveryMode = 2; // Persistent
        properties.ContentType = "application/json";

        _channel.BasicPublish(
            exchange: _exchangeName,
            routingKey: eventName,
            basicProperties: properties,
            body: body);

        _logger.LogInformation("Published event {EventName} with ID {EventId}", eventName, @event.Id);
        
        await Task.CompletedTask;
    }

    private async Task ProcessEvent(string eventName, string message)
    {
        _logger.LogInformation("Processing event: {EventName}", eventName);

        if (!_handlers.TryGetValue(eventName, out var handlerTypes))
        {
            _logger.LogWarning("No handlers for event: {EventName}", eventName);
            return;
        }

        if (!_eventTypes.TryGetValue(eventName, out var eventType))
        {
            _logger.LogWarning("Event type not found: {EventName}", eventName);
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        
        foreach (var handlerType in handlerTypes)
        {
            try
            {
                var handler = scope.ServiceProvider.GetService(handlerType);
                if (handler == null)
                {
                    _logger.LogWarning("Handler {HandlerType} not registered", handlerType.Name);
                    continue;
                }

                var @event = JsonSerializer.Deserialize(message, eventType);
                if (@event == null) continue;

                var handleMethod = handlerType.GetMethod("HandleAsync");
                if (handleMethod != null)
                {
                    await (Task)handleMethod.Invoke(handler, new[] { @event })!;
                    _logger.LogInformation("Event {EventName} handled by {Handler}", eventName, handlerType.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in handler {Handler} for event {EventName}", 
                    handlerType.Name, eventName);
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        _channel?.Close();
        _connection?.Close();
        _disposed = true;
        
        _logger.LogInformation("RabbitMQ connection disposed");
    }
}
