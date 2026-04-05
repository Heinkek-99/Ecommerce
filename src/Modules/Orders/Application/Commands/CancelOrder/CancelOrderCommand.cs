using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Orders.Application.Commands.CancelOrder;

public record CancelOrderCommand(Guid OrderId, Guid UserId) : ICommand<Result>;
