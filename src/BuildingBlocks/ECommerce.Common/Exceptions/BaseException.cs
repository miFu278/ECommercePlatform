namespace ECommerce.Common.Exceptions;

public abstract class BaseException : Exception
{
    public string Code { get; set; }
    public int StatusCode { get; set; }

    protected BaseException(string message, string code, int statusCode) 
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }

    protected BaseException(string message, string code, int statusCode, Exception innerException) 
        : base(message, innerException)
    {
        Code = code;
        StatusCode = statusCode;
    }
}
