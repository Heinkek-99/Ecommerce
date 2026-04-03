using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Orders.Application.Queries.GetOrder;

public record GetOrderQuery(Guid OrderId, Guid UserId) : IQuery<Result<OrderDto>>;

public record OrderDto(
    Guid Id,
    Guid UserId,
    string Status,
    decimal TotalAmount,
    string CurrencyCode,
    DateTime OrderedAt,
    List<OrderItemDto> Items);

public record OrderItemDto(
    Guid VariantId,
    string ProductName,
    string ProductSku,
    int Quantity,
    decimal UnitPrice);
