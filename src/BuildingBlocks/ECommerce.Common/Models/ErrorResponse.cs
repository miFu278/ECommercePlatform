namespace ECommerce.Common.Models;

public class ErrorResponse
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Field { get; set; }
    public Dictionary<string, object>? Details { get; set; }
}

public class ErrorDetail
{
    public bool Success { get; set; } = false;
    public List<ErrorResponse> Errors { get; set; } = new();
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? TraceId { get; set; }
    public string? Path { get; set; }
}
