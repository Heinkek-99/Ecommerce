using Ecommerce.Cart.Application.Services;
using Ecommerce.Shared.Common;

namespace Ecommerce.Cart.Application.Commands.ClearCart;

public class ClearCartHandler
{
    private readonly ICartStorage _storage;

    public ClearCartHandler(ICartStorage storage) => _storage = storage;

    public async Task<Result> Handle(ClearCartCommand cmd, CancellationToken ct)
    {
        await _storage.DeleteAsync(cmd.UserId, ct);
        return Result.Success();
    }
}
