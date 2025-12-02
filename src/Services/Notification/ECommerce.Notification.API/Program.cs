using ECommerce.EventBus.Abstractions;
using ECommerce.EventBus.Events;
using ECommerce.EventBus.InMemory;
using ECommerce.EventBus.RabbitMQ;
using ECommerce.Notification.Application.EventHandlers;
using ECommerce.Notification.Application.Interfaces;
using ECommerce.Notification.Domain.Interfaces;
using ECommerce.Notification.Infrastructure.Configuration;
using ECommerce.Notification.Infrastructure.Data;
using ECommerce.Notification.Infrastructure.Repositories;
using ECommerce.Notification.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Kestrel configuration is handled by ASPNETCORE_URLS environment variable
// Local: http://localhost:5060
// Docker: http://+:8080

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ECommerce Notification Service API",
        Version = "v1",
        Description = "API for sending email notifications with MongoDB logging"
    });
});

// MongoDB Configuration
var mongoSettings = new MongoDbSettings
{
    ConnectionString = builder.Configuration.GetConnectionString("MongoDB") 
        ?? "mongodb://admin:admin123@localhost:27017",
    DatabaseName = builder.Configuration["MongoDB:DatabaseName"] ?? "ECommerce_Notification"
};

builder.Services.AddSingleton(mongoSettings);
builder.Services.AddScoped<IMongoDbContext, MongoDbContext>();

// Repositories
builder.Services.AddScoped<INotificationLogRepository, NotificationLogRepository>();
builder.Services.AddScoped<IEmailLogRepository, EmailLogRepository>();

// Services
builder.Services.AddScoped<IEmailService, EmailService>();

// Event Handlers
builder.Services.AddScoped<OrderCreatedEventHandler>();
builder.Services.AddScoped<PaymentCompletedEventHandler>();

// Event Bus - Use RabbitMQ if configured
var rabbitMQConnectionString = builder.Configuration["RabbitMQ:ConnectionString"];
var rabbitMQQueueName = builder.Configuration["RabbitMQ:QueueName"] ?? "notification_service_queue";

if (!string.IsNullOrEmpty(rabbitMQConnectionString))
{
    builder.Services.AddSingleton<IEventBus>(sp =>
    {
        var logger = sp.GetRequiredService<ILogger<RabbitMQEventBus>>();
        var eventBus = new RabbitMQEventBus(sp, logger, rabbitMQConnectionString, rabbitMQQueueName);
        
        // Subscribe to events
        eventBus.Subscribe<OrderCreatedEvent, OrderCreatedEventHandler>();
        eventBus.Subscribe<PaymentCompletedEvent, PaymentCompletedEventHandler>();
        
        return eventBus;
    });
    Console.WriteLine("✅ Using RabbitMQ EventBus");
}
else
{
    builder.Services.AddSingleton<IEventBus>(sp =>
    {
        var logger = sp.GetRequiredService<ILogger<InMemoryEventBus>>();
        var eventBus = new InMemoryEventBus(sp, logger);
        eventBus.Subscribe<OrderCreatedEvent, OrderCreatedEventHandler>();
        eventBus.Subscribe<PaymentCompletedEvent, PaymentCompletedEventHandler>();
        return eventBus;
    });
    Console.WriteLine("⚠️ Using InMemory EventBus");
}

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification Service API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("✅ Notification Service started");
Console.WriteLine("📧 REST API: http://localhost:5060");
Console.WriteLine("💾 MongoDB: " + mongoSettings.DatabaseName);
Console.WriteLine("📬 Event handlers registered:");
Console.WriteLine("   - OrderCreatedEvent → OrderCreatedEventHandler");
Console.WriteLine("   - PaymentCompletedEvent → PaymentCompletedEventHandler");
Console.ResetColor();

app.Run();
