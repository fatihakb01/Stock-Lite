using Application.Entities.Core.Products.DTOs;
using FluentValidation;

namespace Application.Entities.Core.Products.Validators;

public class ProductDtoValidator : AbstractValidator<BaseProductDto>
{
    public ProductDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage("Product name is required")
            .MaximumLength(100)
                .WithMessage("Product name can only have a maximum of 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500)
                .WithMessage("Product description can only have a maximum of 500 characters");

        RuleFor(x => x.SKU)
            .MaximumLength(100)
                .WithMessage("Product SKU can only have a maximum of 100 characters");

        RuleFor(x => x.Price)
            .NotEmpty()
                .WithMessage("Product's price is required")
            .Must(amount => decimal.Round(amount, 2) == amount)
                .WithMessage("Product's price can only have up to 2 decimal places")
            .Must(amount => amount > 0)
                .WithMessage("Product's price should be greater than 0");
    }
}
