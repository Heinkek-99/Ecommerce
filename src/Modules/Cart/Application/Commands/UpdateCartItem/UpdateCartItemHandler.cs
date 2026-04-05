using Ecommerce.Cart.Application.Services;
using Ecommerce.Shared.Common;

namespace Ecommerce.Cart.Application.Commands.UpdateCartItem;

public class UpdateCartItemHandler
{
    private readonly ICartStorage _storage;

    public UpdateCartItemHandler(ICartStorage storage) => _storage = storage;

    public async Task<Result> Handle(UpdateCartItemCommand cmd, CancellationToken ct)
    {
        if (cmd.NewQuantity < 0)
            return Result.Failure("Quantity cannot be negative.");

        var cart = await _storage.GetAsync(cmd.UserId, ct);
        if (cart is null)
            return Result.Failure("Cart not found.");

        cart.UpdateQuantity(cmd.VariantId, cmd.NewQuantity);
        await _storage.SetAsync(cart, ct);
        return Result.Success();
    }
}
