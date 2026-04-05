using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Orders.Application.Commands.PrepareOrder;

public record PrepareOrderCommand(Guid OrderId, Guid SellerId) : ICommand<Result>;
