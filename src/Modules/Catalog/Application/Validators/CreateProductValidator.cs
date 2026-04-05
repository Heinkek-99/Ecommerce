using Ecommerce.Catalog.Application.Commands.CreateProduct;
using FluentValidation;

namespace Ecommerce.Catalog.Application.Validators;

public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(500);
        RuleFor(x => x.BasePrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SellerId).NotEmpty();
    }
}
