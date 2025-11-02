namespace ECommerce.Common.Exceptions;

public class ForbiddenException : BaseException
{
    public ForbiddenException(string message = "Access forbidden") 
        : base(message, "FORBIDDEN", 403)
    {
    }
}
