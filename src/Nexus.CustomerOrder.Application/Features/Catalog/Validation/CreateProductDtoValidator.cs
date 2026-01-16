using FluentValidation;
using Nexus.CustomerOrder.Application.Features.Catalog.Models;
using Nexus.Shared.Kernel.Extensions;

namespace Nexus.CustomerOrder.Application.Features.Catalog.Validation;

public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(X => X.Sku)
            .Must(x => x.IsPresent())
            .WithMessage("SKU is required")
            .WithErrorCode("PRODUCT_SKU_REQUIRED")
            .MaximumLength(50)
            .WithMessage("SKU must not exceed 50 characters")
            .WithErrorCode("PRODUCT_SKU_TOO_LONG");

        RuleFor(x => x.Name)
            .Must(x => x.IsPresent())
            .WithMessage("Product name is required")
            .WithErrorCode("PRODUCT_NAME_REQUIRED")
            .MaximumLength(200)
            .WithMessage("Product name must not exceed 200 characters")
            .WithErrorCode("PRODUCT_NAME_TOO_LONG");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => x.Description != null && x.Description.IsPresent())
            .WithMessage("Description must not exceed 2000 characters")
            .WithErrorCode("PRODUCT_DESCRIPTION_TOO_LONG");

        RuleFor(x => x.BasePrice)
            .GreaterThan(0)
            .WithMessage("Base price must be greater than zero")
            .WithErrorCode("PRODUCT_PRICE_INVALID")
            .Must(price => Math.Round(price, 2) == price)
            .WithMessage("Base price must have at most 2 decimal places")
            .WithErrorCode("PRODUCT_PRICE_PRECISION_INVALID");

        RuleFor(x => x.Category)
            .Must(x => x.IsPresent())
            .WithMessage("Category is required")
            .WithErrorCode("PRODUCT_CATEGORY_REQUIRED")
            .MaximumLength(100)
            .WithMessage("Category must not exceed 100 characters")
            .WithErrorCode("PRODUCT_CATEGORY_TOO_LONG");
    }
}