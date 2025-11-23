using ECommerce.User.Application.DTOs;
using FluentValidation;

namespace ECommerce.User.Application.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.Username)
            .MinimumLength(3).WithMessage("Username must be at least 3 characters")
            .MaximumLength(50).WithMessage("Username must not exceed 50 characters")
            .Matches(@"^[a-zA-Z0-9_-]+$")
            .WithMessage("Username can only contain letters, numbers, underscore and hyphen (e.g., john_doe, user-123)")
            .When(x => !string.IsNullOrEmpty(x.Username));

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter (A-Z)")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter (a-z)")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number (0-9)")
            .Matches(@"[\W_]").WithMessage("Password must contain at least one special character (e.g., @, #, $, !, %)");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required")
            .Equal(x => x.Password).WithMessage("Passwords do not match");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("Invalid phone number format. Use E.164 format (e.g., +84375331022 or +1234567890)")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.UtcNow)
            .WithMessage("Date of birth must be in the past")
            .LessThanOrEqualTo(DateTime.UtcNow.AddYears(-13))
            .WithMessage("You must be at least 13 years old to register")
            .GreaterThan(DateTime.UtcNow.AddYears(-120))
            .WithMessage("Date of birth must be within the last 120 years. Use ISO 8601 format (e.g., 2005-08-27)")
            .When(x => x.DateOfBirth.HasValue);
    }
}
