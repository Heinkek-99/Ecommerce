using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Orders.Application.Queries.GetSellerOrders;

public record GetSellerOrdersQuery(
    Guid SellerId,
    string? Status,
    int Page,
    int PageSize = 20
) : IQuery<Result<PagedList<SellerOrderDto>>>;

public record SellerOrderDto(
    Guid OrderId,
    Guid UserId,
    string Status,
    decimal TotalAmount,
    string CurrencyCode,
    DateTime OrderedAt,
    List<SellerOrderItemDto> Items);

public record SellerOrderItemDto(
    Guid VariantId,
    string ProductName,
    string ProductSku,
    int Quantity,
    decimal UnitPrice);
