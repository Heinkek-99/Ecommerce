using Ecommerce.Cart.Domain;

namespace Ecommerce.Cart.Application.Services;

public interface ICartStorage
{
    Task<ShoppingCart?> GetAsync(Guid userId, CancellationToken ct);
    Task SetAsync(ShoppingCart cart, CancellationToken ct);
    Task DeleteAsync(Guid userId, CancellationToken ct);
    Task<bool> ExistsAsync(Guid userId, CancellationToken ct);
}
