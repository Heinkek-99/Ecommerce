using Ecommerce.Cart.Application.Services;
using Ecommerce.Shared.Common;

namespace Ecommerce.Cart.Application.Commands.RemoveFromCart;

public class RemoveFromCartHandler
{
    private readonly ICartStorage _storage;

    public RemoveFromCartHandler(ICartStorage storage) => _storage = storage;

    public async Task<Result> Handle(RemoveFromCartCommand cmd, CancellationToken ct)
    {
        var cart = await _storage.GetAsync(cmd.UserId, ct);
        if (cart is null)
            return Result.Failure("Cart not found.");

        cart.RemoveItem(cmd.VariantId);
        await _storage.SetAsync(cart, ct);
        return Result.Success();
    }
}
