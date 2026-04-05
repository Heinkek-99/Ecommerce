using Ecommerce.Cart.Application.Services;
using Ecommerce.Cart.Domain;
using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Cart.Application.Commands.AddToCart;

public class AddToCartHandler
{
    private readonly ICartStorage _storage;
    private readonly ICatalogIntegrationService _catalog;

    public AddToCartHandler(ICartStorage storage, ICatalogIntegrationService catalog)
    {
        _storage = storage;
        _catalog = catalog;
    }

    public async Task<Result> Handle(AddToCartCommand cmd, CancellationToken ct)
    {
        if (cmd.Quantity <= 0)
            return Result.Failure("Quantity must be greater than zero.");

        var variantInfo = await _catalog.GetVariantInfoAsync(cmd.VariantId, ct);
        if (variantInfo is null)
            return Result.Failure($"Variant {cmd.VariantId} not found.");

        if (variantInfo.StockQuantity < cmd.Quantity)
            return Result.Failure($"Insufficient stock. Available: {variantInfo.StockQuantity}.");

        var cart = await _storage.GetAsync(cmd.UserId, ct) ?? new ShoppingCart(cmd.UserId);

        cart.AddItem(new CartItem(
            variantInfo.VariantId,
            variantInfo.ProductId,
            variantInfo.SellerId,
            variantInfo.ProductName,
            variantInfo.Sku,
            variantInfo.Price,
            cmd.Quantity,
            variantInfo.ImageUrl));

        await _storage.SetAsync(cart, ct);
        return Result.Success();
    }
}
