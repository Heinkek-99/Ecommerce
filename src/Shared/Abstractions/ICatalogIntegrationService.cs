using Ecommerce.Shared.Common;

namespace Ecommerce.Shared.Abstractions;

public interface ICatalogIntegrationService : IModuleIntegrationService
{
    Task<ProductVariantInfo?> GetVariantInfoAsync(Guid variantId, CancellationToken ct);
    Task<Result> DecrementStockAsync(IEnumerable<(Guid VariantId, int Quantity)> items, CancellationToken ct);
}
