using ECommerce.User.Application.DTOs;
using FluentValidation;

namespace ECommerce.User.Application.Validators;

public class CreateAddressDtoValidator : AbstractValidator<CreateAddressDto>
{
    public CreateAddressDtoValidator()
    {
        RuleFor(x => x.AddressType)
            .NotEmpty().WithMessage("Address type is required")
            .Must(type => type == "shipping" || type == "billing")
            .WithMessage("Address type must be either 'shipping' or 'billing'");

        RuleFor(x => x.StreetAddress)
            .NotEmpty().WithMessage("Street address is required")
            .MaximumLength(255).WithMessage("Street address must not exceed 255 characters");

        RuleFor(x => x.Apartment)
            .MaximumLength(50).WithMessage("Apartment must not exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Apartment));

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(100).WithMessage("City must not exceed 100 characters");

        RuleFor(x => x.StateProvince)
            .MaximumLength(100).WithMessage("State/Province must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.StateProvince));

        RuleFor(x => x.PostalCode)
            .NotEmpty().WithMessage("Postal code is required")
            .MaximumLength(20).WithMessage("Postal code must not exceed 20 characters");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required")
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters");
    }
}
