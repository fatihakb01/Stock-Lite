using Application.Entities.Core.Products.Features.Commands;
using Domain.Entities.Core;
using Domain.Interfaces;
using FluentValidation;

namespace Application.Entities.Core.Products.Validators;

public class EditProductValidator : AbstractValidator<EditProduct.Command>
{
    public EditProductValidator(IUniquenessChecker<Product> uniquenessChecker, IRepository<Product> repository)
    {
        // General rules
        RuleFor(x => x.ProductDto).SetValidator(new ProductDtoValidator());

        // Check for unique Name and SKU
        RuleFor(x => x.ProductDto.Name)
            .MustAsync(async (command, name, ct) =>
            {
                var existing = await repository.GetByIdAsync(command.Id, ct);
                if (existing == null) return true;
                if (existing.Name == name) return true;
                return await uniquenessChecker.IsUniqueAsync(c => c.Name == name, ct);
            })
            .When(x => !string.IsNullOrWhiteSpace(x.ProductDto.Name))
            .WithMessage("Product name must be unique");
        
        RuleFor(x => x.ProductDto.SKU)
            .MustAsync(async (command, sku, ct) =>
            {
                var existing = await repository.GetByIdAsync(command.Id, ct);
                if (existing == null) return true;
                if (existing.SKU == sku) return true;
                return await uniquenessChecker.IsUniqueAsync(c => c.SKU == sku, ct);
            })
            .When(x => !string.IsNullOrWhiteSpace(x.ProductDto.SKU))
            .WithMessage("Product's SKU must be unique");
    }
}
