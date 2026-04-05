using Ecommerce.Cart.Application.Services;
using Ecommerce.Shared.Common;

namespace Ecommerce.Cart.Application.Queries.GetCart;

public class GetCartHandler
{
    private readonly ICartStorage _storage;

    public GetCartHandler(ICartStorage storage) => _storage = storage;

    public async Task<Result<ShoppingCartDto>> Handle(GetCartQuery query, CancellationToken ct)
    {
        var cart = await _storage.GetAsync(query.UserId, ct);
        if (cart is null)
            return Result.Success(new ShoppingCartDto([], 0m, 0));

        var items = cart.Items
            .Select(i => new CartItemDto(
                i.VariantId,
                i.ProductId,
                i.SellerId,
                i.ProductName,
                i.Sku,
                i.UnitPrice,
                i.Quantity,
                i.UnitPrice * i.Quantity,
                i.ImageUrl))
            .ToList();

        return Result.Success(new ShoppingCartDto(
            items,
            cart.GetTotal(),
            cart.Items.Sum(i => i.Quantity)));
    }
}
