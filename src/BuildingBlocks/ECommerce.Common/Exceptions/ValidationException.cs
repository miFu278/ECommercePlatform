namespace ECommerce.Common.Exceptions;

public class ValidationException : BaseException
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(Dictionary<string, string[]> errors) 
        : base("One or more validation errors occurred", "VALIDATION_ERROR", 400)
    {
        Errors = errors;
    }

    public ValidationException(string field, string error) 
        : base($"Validation failed for field '{field}'", "VALIDATION_ERROR", 400)
    {
        Errors = new Dictionary<string, string[]>
        {
            { field, new[] { error } }
        };
    }
}
