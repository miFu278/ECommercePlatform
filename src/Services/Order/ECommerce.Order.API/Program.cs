using System.Text;
using ECommerce.EventBus.Abstractions;
using ECommerce.EventBus.Events;
using ECommerce.EventBus.InMemory;
using ECommerce.EventBus.RabbitMQ;
using ECommerce.Order.Application.EventHandlers;
using ECommerce.Order.Application.Interfaces;
using ECommerce.Order.Application.Mappings;
using ECommerce.Order.Application.Services;
using ECommerce.Order.Application.Validators;
using ECommerce.Order.Domain.Interfaces;
using ECommerce.Order.Infrastructure.Data;
using ECommerce.Order.Infrastructure.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// Fix PostgreSQL DateTime issue
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Kestrel configuration is handled by ASPNETCORE_URLS environment variable
// Local: http://localhost:5040;http://localhost:5041
// Docker: http://+:8080;http://+:8081

// Add services to the container
builder.Services.AddGrpc();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ECommerce Order Service API",
        Version = "v1",
        Description = "API for order management and processing"
    });

    // Add JWT Authentication
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderDtoValidator>();

// HttpContextAccessor for CurrentUserService
builder.Services.AddHttpContextAccessor();

// Database
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// AutoMapper
builder.Services.AddAutoMapper(typeof(OrderMappingProfile));

// Current User Service (for getting userId from JWT)
builder.Services.AddScoped<ECommerce.Shared.Abstractions.Interceptors.ICurrentUserService, CurrentUserService>();

// Unit of Work & Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services
builder.Services.AddScoped<IOrderService, OrderService>();

// gRPC Clients
builder.Services.AddScoped<ECommerce.Order.Infrastructure.GrpcClients.IProductGrpcClient, ECommerce.Order.Infrastructure.GrpcClients.ProductGrpcClient>();
builder.Services.AddScoped<ECommerce.Order.Infrastructure.GrpcClients.IUserGrpcClient, ECommerce.Order.Infrastructure.GrpcClients.UserGrpcClient>();

// HttpClient for ShoppingCart Service
builder.Services.AddHttpClient<IShoppingCartService, ShoppingCartService>(client =>
{
    var baseUrl = builder.Configuration["Services:ShoppingCart"] ?? "http://localhost:5002";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// HttpClient for Payment Service
builder.Services.AddHttpClient<IPaymentService, ECommerce.Order.Infrastructure.Services.PaymentService>(client =>
{
    var baseUrl = builder.Configuration["Services:Payment"] ?? "http://localhost:5004";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Event Handlers
builder.Services.AddScoped<PaymentCompletedEventHandler>();

// Event Bus - Use RabbitMQ if configured, otherwise InMemory
var rabbitMQConnectionString = builder.Configuration["RabbitMQ:ConnectionString"];
var rabbitMQQueueName = builder.Configuration["RabbitMQ:QueueName"] ?? "order_service_queue";

if (!string.IsNullOrEmpty(rabbitMQConnectionString))
{
    builder.Services.AddSingleton<IEventBus>(sp =>
    {
        var logger = sp.GetRequiredService<ILogger<RabbitMQEventBus>>();
        var eventBus = new RabbitMQEventBus(sp, logger, rabbitMQConnectionString, rabbitMQQueueName);
        
        // Subscribe to events
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
        eventBus.Subscribe<PaymentCompletedEvent, PaymentCompletedEventHandler>();
        return eventBus;
    });
    Console.WriteLine("⚠️ Using InMemory EventBus (RabbitMQ not configured)");
}

// JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
var key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

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
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Service API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Map gRPC Service
app.MapGrpcService<ECommerce.Order.API.Grpc.OrderGrpcService>();

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("✅ Order Service started");
Console.WriteLine("📦 REST API: http://localhost:5040");
Console.WriteLine("🔗 gRPC: http://localhost:5041");
Console.ResetColor();

app.Run();
