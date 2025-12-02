using ECommerce.EventBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ECommerce.EventBus.RabbitMQ;

public static class RabbitMQExtensions
{
    public static IServiceCollection AddRabbitMQEventBus(
        this IServiceCollection services,
        string connectionString,
        string queueName)
    {
        services.AddSingleton<IEventBus>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<RabbitMQEventBus>>();
            return new RabbitMQEventBus(sp, logger, connectionString, queueName);
        });

        return services;
    }
}
