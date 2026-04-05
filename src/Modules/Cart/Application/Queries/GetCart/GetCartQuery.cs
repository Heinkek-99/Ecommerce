using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Cart.Application.Queries.GetCart;

public record GetCartQuery(Guid UserId) : IQuery<Result<ShoppingCartDto>>;

public record ShoppingCartDto(
    List<CartItemDto> Items,
    decimal SubTotal,
    int ItemCount);

public record CartItemDto(
    Guid VariantId,
    Guid ProductId,
    Guid SellerId,
    string ProductName,
    string Sku,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal,
    string? ImageUrl);
