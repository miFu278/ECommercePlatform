namespace ECommerce.Common.Exceptions;

public class BusinessException : BaseException
{
    public BusinessException(string message) 
        : base(message, "BUSINESS_ERROR", 400)
    {
    }

    public BusinessException(string message, string code) 
        : base(message, code, 400)
    {
    }
}
