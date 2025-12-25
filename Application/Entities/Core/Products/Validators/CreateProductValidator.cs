using Application.Entities.Core.Products.Features.Commands;
using FluentValidation;

namespace Application.Entities.Core.Products.Validators;

public class CreateProductValidator : AbstractValidator<CreateProduct.Command>
{
    public CreateProductValidator()
    {
        // General rules
        RuleFor(x => x.ProductDto).SetValidator(new ProductDtoValidator());
    }
}
