using ECommerce.EventBus.Abstractions;
using ECommerce.EventBus.InMemory;
using ECommerce.Notification.Application.EventHandlers;
using ECommerce.Notification.Application.Interfaces;
using ECommerce.Notification.Domain.Interfaces;
using ECommerce.Notification.Infrastructure.Configuration;
using ECommerce.Notification.Infrastructure.Data;
using ECommerce.Notification.Infrastructure.Repositories;
using ECommerce.Notification.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

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

// Event Bus (In-Memory for now)
builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();

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
Console.WriteLine("✅ Notification Service started with MongoDB");
Console.WriteLine("📧 Email notifications enabled");
Console.WriteLine("💾 Logging to MongoDB: ECommerce_Notification");
Console.ResetColor();

app.Run();
