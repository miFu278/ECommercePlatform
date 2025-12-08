using ECommerce.ShoppingCart.Application.Interfaces;
using ECommerce.ShoppingCart.Application.Services;
using ECommerce.ShoppingCart.Domain.Interfaces;
using ECommerce.ShoppingCart.Infrastructure.Data;
using ECommerce.ShoppingCart.Infrastructure.Repositories;
using ECommerce.ShoppingCart.Infrastructure.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Shopping Cart API", Version = "v1" });
});

// Redis Configuration
var redisConnection = builder.Configuration.GetConnectionString("Redis") 
    ?? "localhost:6379";

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    try
    {
        var configuration = ConfigurationOptions.Parse(redisConnection);
        configuration.AbortOnConnectFail = false;
        configuration.ConnectTimeout = 10000;
        configuration.SyncTimeout = 10000;
        configuration.ConnectRetry = 3;
        configuration.KeepAlive = 60;
        configuration.AllowAdmin = false;
        
        var muxer = ConnectionMultiplexer.Connect(configuration);
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✅ Redis ConnectionMultiplexer created");
        Console.ResetColor();
        
        return muxer;
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ Redis connection failed: {ex.Message}");
        Console.ResetColor();
        throw;
    }
});

builder.Services.AddSingleton<RedisContext>();

// AutoMapper
builder.Services.AddAutoMapper(
    typeof(Program).Assembly,
    typeof(ECommerce.ShoppingCart.Application.Mappings.CartMappingProfile).Assembly);

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<ECommerce.ShoppingCart.Application.Validators.AddToCartDtoValidator>();

// Repositories
builder.Services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();

// Services
builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();

// HTTP Client for Product Service
builder.Services.AddHttpClient<IProductService, ProductService>(client =>
{
    var productServiceUrl = builder.Configuration["Services:ProductService:Url"] 
        ?? "http://localhost:5001";
    client.BaseAddress = new Uri(productServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

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
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.MapControllers();

// Test Redis connection
try
{
    var redis = app.Services.GetRequiredService<IConnectionMultiplexer>();
    var db = redis.GetDatabase();
    await db.PingAsync();
    Console.WriteLine("✅ Redis connection successful!");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️  Redis connection failed: {ex.Message}");
    Console.WriteLine("⚠️  Application will continue but cart operations will fail.");
    Console.WriteLine("💡 Start Redis: docker run -d --name redis -p 6379:6379 redis:7-alpine");
}

app.Run();
