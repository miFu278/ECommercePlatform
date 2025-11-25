using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.Configuration.File;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Load Ocelot configuration based on environment
var env = builder.Environment.EnvironmentName;
builder.Configuration.AddJsonFile($"ocelot.{env}.json", optional: false, reloadOnChange: true);

// JWT Authentication - Validate at Gateway level
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "your-super-secret-key-min-32-characters-long-for-production";
var key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "ECommerce.UserService",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "ECommerce.Clients",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // Add custom headers after successful validation
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
                if (claimsIdentity != null)
                {
                    // Extract claims and add to headers for downstream services
                    var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var email = claimsIdentity.FindFirst(ClaimTypes.Email)?.Value;
                    var role = claimsIdentity.FindFirst(ClaimTypes.Role)?.Value;

                    if (!string.IsNullOrEmpty(userId))
                        context.HttpContext.Items["UserId"] = userId;
                    if (!string.IsNullOrEmpty(email))
                        context.HttpContext.Items["UserEmail"] = email;
                    if (!string.IsNullOrEmpty(role))
                        context.HttpContext.Items["UserRole"] = role;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Add Ocelot services
builder.Services.AddOcelot();

var app = builder.Build();

// Custom startup banner
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine(@"
╔═══════════════════════════════════════════════════════════╗
║                   API GATEWAY - OCELOT                    ║
╚═══════════════════════════════════════════════════════════╝
");
Console.ResetColor();

// Display configuration info
var ocelotConfig = builder.Configuration.GetSection("Routes").Get<List<FileRoute>>();
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"✓ Environment: {env}");
Console.WriteLine($"✓ Gateway URL: http://localhost:5050");
Console.WriteLine($"✓ Metrics: http://localhost:5050/metrics");
Console.WriteLine($"✓ Prometheus: http://localhost:9090");
Console.WriteLine($"✓ Grafana: http://localhost:3000");
Console.WriteLine($"✓ Routes loaded: {ocelotConfig?.Count ?? 0}");
Console.ResetColor();

if (ocelotConfig != null && ocelotConfig.Any())
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("\n📍 Available Routes:");
    Console.ResetColor();
    foreach (var route in ocelotConfig)
    {
        var downstream = route.DownstreamHostAndPorts?.FirstOrDefault();
        Console.WriteLine($"  {route.UpstreamHttpMethod?.FirstOrDefault() ?? "GET",-6} {route.UpstreamPathTemplate,-30} → {downstream?.Host}:{downstream?.Port}{route.DownstreamPathTemplate}");
    }
}

Console.WriteLine("\n" + new string('─', 63));
Console.ForegroundColor = ConsoleColor.Gray;
Console.WriteLine("Press Ctrl+C to shutdown");
Console.ResetColor();
Console.WriteLine(new string('─', 63) + "\n");

// Enable Prometheus metrics
app.UseMetricServer(); // Expose /metrics endpoint
app.UseHttpMetrics();  // Track HTTP metrics

// JWT Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Middleware to add user info headers to downstream requests
app.Use(async (context, next) =>
{
    // Add user info from JWT to headers for downstream services
    if (context.Items.TryGetValue("UserId", out var userId))
        context.Request.Headers["X-User-Id"] = userId?.ToString();
    if (context.Items.TryGetValue("UserEmail", out var email))
        context.Request.Headers["X-User-Email"] = email?.ToString();
    if (context.Items.TryGetValue("UserRole", out var role))
        context.Request.Headers["X-User-Role"] = role?.ToString();

    await next();
});

// Use Ocelot middleware
await app.UseOcelot();

app.Run();
