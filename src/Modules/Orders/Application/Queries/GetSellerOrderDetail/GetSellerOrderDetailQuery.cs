using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Orders.Application.Queries.GetSellerOrderDetail;

public record GetSellerOrderDetailQuery(Guid OrderId, Guid SellerId) : IQuery<Result<SellerOrderDetailDto>>;

public record SellerOrderDetailDto(
    Guid OrderId,
    Guid UserId,
    string Status,
    decimal TotalAmount,
    string CurrencyCode,
    Guid AddressId,
    DateTime OrderedAt,
    DateTime UpdatedAt,
    List<SellerOrderDetailItemDto> Items);

public record SellerOrderDetailItemDto(
    Guid VariantId,
    string ProductName,
    string ProductSku,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);
