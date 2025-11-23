namespace ECommerce.Common.Exceptions;

/// <summary>
/// Exception for HTTP 409 Conflict - Resource already exists or state conflict
/// </summary>
public class ConflictException : BaseException
{
    public ConflictException(string message) 
        : base(message, "CONFLICT", 409)
    {
    }

    public ConflictException(string message, string code) 
        : base(message, code, 409)
    {
    }
}
