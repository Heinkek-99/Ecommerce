using Ecommerce.Orders.Application.Commands.PlaceOrder;
using FluentValidation;

namespace Ecommerce.Orders.Application.Validators;

public class PlaceOrderValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.AddressId).NotEmpty();
        RuleFor(x => x.CurrencyCode).NotEmpty().Length(3);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("Order must have at least one item.");
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.Quantity).GreaterThan(0);
            line.RuleFor(l => l.UnitPrice).GreaterThanOrEqualTo(0);
            line.RuleFor(l => l.ProductName).NotEmpty();
            line.RuleFor(l => l.ProductSku).NotEmpty();
        });
    }
}
