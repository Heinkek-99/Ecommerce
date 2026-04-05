using Ecommerce.Orders.Domain;
using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Orders.Application.Commands.PlaceOrder;

public record PlaceOrderCommand(
    Guid UserId,
    Guid AddressId,
    string CurrencyCode,
    string? PromoCode,
    List<OrderItemLine> Lines
) : ICommand<Result<Guid>>;
