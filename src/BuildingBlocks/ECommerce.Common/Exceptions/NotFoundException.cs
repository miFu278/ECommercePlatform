namespace ECommerce.Common.Exceptions;

public class NotFoundException : BaseException
{
    public NotFoundException(string message) 
        : base(message, "NOT_FOUND", 404)
    {
    }

    public NotFoundException(string entityName, object key) 
        : base($"{entityName} with id '{key}' was not found", "NOT_FOUND", 404)
    {
    }
}
