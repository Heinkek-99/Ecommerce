using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Orders.Application.Commands.Checkout;

public record CheckoutCommand(
    Guid UserId,
    Guid AddressId,
    string CurrencyCode,
    List<CheckoutItemLine> Items,
    string? SellerStripeAccountId = null
) : ICommand<Result<CheckoutResult>>;

public record CheckoutItemLine(
    Guid VariantId,
    Guid ProductId,
    Guid SellerId,
    string ProductName,
    string ProductSku,
    decimal UnitPrice,
    int Quantity);

public record CheckoutResult(Guid OrderId, string ClientSecret);
