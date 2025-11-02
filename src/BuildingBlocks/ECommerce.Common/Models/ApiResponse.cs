namespace ECommerce.Common.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> SuccessResponse(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message ?? "Operation completed successfully"
        };
    }

    public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }

    public static ApiResponse<T> ErrorResponse(List<string> errors)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = "Operation failed",
            Errors = errors
        };
    }
}

// Non-generic version for operations without data
public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse SuccessResult(string? message = null)
    {
        return new ApiResponse
        {
            Data = null,
            Message = message ?? "Operation completed successfully"
        };
    }

    public static new ApiResponse ErrorResponse(string message, List<string>? errors = null)
    {
        return new ApiResponse
        {
            Data = null,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}
