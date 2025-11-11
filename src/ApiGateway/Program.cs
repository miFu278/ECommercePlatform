using Ocelot.Configuration.File;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Load Ocelot configuration based on environment
var env = builder.Environment.EnvironmentName;
builder.Configuration.AddJsonFile($"ocelot.{env}.json", optional: false, reloadOnChange: true);

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

// Use Ocelot middleware
await app.UseOcelot();

app.Run();
