using ECommerce.Order.Application.DTOs;
using FluentValidation;

namespace ECommerce.Order.Application.Validators;

public class UpdateOrderStatusDtoValidator : AbstractValidator<UpdateOrderStatusDto>
{
    public UpdateOrderStatusDtoValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid order status");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notes cannot exceed 500 characters");

        RuleFor(x => x.TrackingNumber)
            .MaximumLength(100)
            .WithMessage("Tracking number cannot exceed 100 characters");
    }
}
