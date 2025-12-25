using Application.Entities.Core.Products.Features.Commands;
using FluentValidation;

namespace Application.Entities.Core.Products.Validators;

public class EditProductValidator : AbstractValidator<EditProduct.Command>
{
    public EditProductValidator()
    {
        // General rules
        RuleFor(x => x.ProductDto).SetValidator(new ProductDtoValidator());
    }
}
