using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Orders.Application.Commands.ShipOrder;

public record ShipOrderCommand(
    Guid OrderId,
    Guid SellerId,
    string TrackingNumber,
    string Carrier
) : ICommand<Result>;
