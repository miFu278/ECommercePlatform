using Microsoft.Extensions.Logging;

namespace ECommerce.Logging;

public static class LoggingExtensions
{
    public static ILogger LogWithContext(this ILogger logger, string correlationId)
    {
        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        }))
        {
            return logger;
        }
    }

    public static void LogUserAction(this ILogger logger, string userId, string action, string? details = null)
    {
        logger.LogInformation(
            "User {UserId} performed action: {Action}. Details: {Details}",
            userId, action, details ?? "N/A"
        );
    }

    public static void LogApiRequest(this ILogger logger, string method, string path, string? userId = null)
    {
        logger.LogInformation(
            "API Request: {Method} {Path} by User: {UserId}",
            method, path, userId ?? "Anonymous"
        );
    }

    public static void LogApiResponse(this ILogger logger, string method, string path, int statusCode, long elapsedMs)
    {
        logger.LogInformation(
            "API Response: {Method} {Path} returned {StatusCode} in {ElapsedMs}ms",
            method, path, statusCode, elapsedMs
        );
    }
}
