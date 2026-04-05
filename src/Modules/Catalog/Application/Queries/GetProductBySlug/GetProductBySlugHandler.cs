using Ecommerce.Catalog.Infrastructure;
using Ecommerce.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Catalog.Application.Queries.GetProductBySlug;

public class GetProductBySlugHandler
{
    private readonly CatalogDbContext _db;

    public GetProductBySlugHandler(CatalogDbContext db) => _db = db;

    public async Task<Result<ProductDetailDto>> Handle(
        GetProductBySlugQuery query,
        CancellationToken ct)
    {
        var product = await _db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == query.Slug, ct);

        if (product is null)
            return Result.Failure<ProductDetailDto>($"Product '{query.Slug}' not found.");

        var variants = await _db.ProductVariants
            .AsNoTracking()
            .Where(v => v.ProductId == product.Id)
            .Select(v => new VariantDto(v.Id, v.Sku, v.Color, v.Size, v.Price, v.StockQuantity))
            .ToListAsync(ct);

        string? categoryName = null;
        if (product.CategoryId.HasValue)
            categoryName = await _db.Categories
                .AsNoTracking()
                .Where(c => c.Id == product.CategoryId)
                .Select(c => c.Name)
                .FirstOrDefaultAsync(ct);

        var dto = new ProductDetailDto(
            product.Id,
            product.SellerId,
            product.SellerName,
            product.Name,
            product.Slug,
            product.Description,
            product.BasePrice,
            product.MainImageUrl,
            product.IsActive,
            product.CreatedAt,
            product.CategoryId,
            categoryName,
            variants);

        return Result.Success(dto);
    }
}
