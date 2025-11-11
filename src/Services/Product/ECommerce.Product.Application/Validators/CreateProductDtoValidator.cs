using ECommerce.Product.Application.DTOs;
using FluentValidation;

namespace ECommerce.Product.Application.Validators;

public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU is required")
            .MaximumLength(50).WithMessage("SKU must not exceed 50 characters");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required")
            .MaximumLength(200).WithMessage("Slug must not exceed 200 characters")
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Slug must be lowercase alphanumeric with hyphens only");

        RuleFor(x => x.ShortDescription)
            .NotEmpty().WithMessage("Short description is required")
            .MaximumLength(500).WithMessage("Short description must not exceed 500 characters");

        RuleFor(x => x.LongDescription)
            .NotEmpty().WithMessage("Long description is required")
            .MaximumLength(5000).WithMessage("Long description must not exceed 5000 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.CompareAtPrice)
            .GreaterThan(x => x.Price)
            .When(x => x.CompareAtPrice.HasValue)
            .WithMessage("Compare at price must be greater than price");

        RuleFor(x => x.CostPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Cost price must be greater than or equal to 0");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required");

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Weight must be greater than 0");

        RuleFor(x => x.Inventory)
            .NotNull().WithMessage("Inventory information is required");

        RuleFor(x => x.Inventory.Stock)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Inventory != null)
            .WithMessage("Stock must be greater than or equal to 0");

        RuleFor(x => x.Inventory.LowStockThreshold)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Inventory != null)
            .WithMessage("Low stock threshold must be greater than or equal to 0");

        RuleFor(x => x.Images)
            .Must(images => images == null || images.Any(i => i.IsPrimary))
            .When(x => x.Images != null && x.Images.Any())
            .WithMessage("At least one image must be marked as primary");

        RuleFor(x => x.Dimensions)
            .Must(d => d!.Length > 0 && d.Width > 0 && d.Height > 0)
            .When(x => x.Dimensions != null)
            .WithMessage("All dimensions must be greater than 0");
    }
}
