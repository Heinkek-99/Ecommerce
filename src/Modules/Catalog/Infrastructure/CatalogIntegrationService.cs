using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Catalog.Infrastructure;

public class CatalogIntegrationService : ICatalogIntegrationService
{
    private readonly CatalogDbContext _db;

    public CatalogIntegrationService(CatalogDbContext db) => _db = db;

    public async Task<ProductVariantInfo?> GetVariantInfoAsync(Guid variantId, CancellationToken ct)
    {
        return await _db.ProductVariants
            .AsNoTracking()
            .Where(v => v.Id == variantId)
            .Join(
                _db.Products.AsNoTracking(),
                v => v.ProductId,
                p => p.Id,
                (v, p) => new ProductVariantInfo(
                    v.Id,
                    p.Id,
                    p.SellerId,
                    p.Name,
                    v.Sku,
                    v.Price ?? p.BasePrice,
                    v.StockQuantity,
                    p.MainImageUrl))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Result> DecrementStockAsync(
        IEnumerable<(Guid VariantId, int Quantity)> items,
        CancellationToken ct)
    {
        var itemList = items.ToList();
        var variantIds = itemList.Select(i => i.VariantId).ToList();

        var variants = await _db.ProductVariants
            .Where(v => variantIds.Contains(v.Id))
            .ToListAsync(ct);

        foreach (var (variantId, quantity) in itemList)
        {
            var variant = variants.FirstOrDefault(v => v.Id == variantId);
            if (variant is null)
                return Result.Failure($"Variant {variantId} not found.");
            if (variant.StockQuantity < quantity)
                return Result.Failure($"Insufficient stock for variant {variantId}. Available: {variant.StockQuantity}.");
            variant.AdjustStock(-quantity);
        }

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
