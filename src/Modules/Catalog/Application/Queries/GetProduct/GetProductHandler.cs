using Ecommerce.Catalog.Infrastructure;
using Ecommerce.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Catalog.Application.Queries.GetProduct;

public class GetProductHandler
{
    private readonly CatalogDbContext _db;

    public GetProductHandler(CatalogDbContext db)
    {
        _db = db;
    }

    public async Task<Result<ProductDto>> Handle(
        GetProductQuery query,
        CancellationToken ct)
    {
        var product = await _db.Products
            .AsNoTracking()
            .Where(p => p.Id == query.ProductId)
            .Select(p => new ProductDto(
                p.Id,
                p.SellerId,
                p.CategoryId,
                p.Name,
                p.Description,
                p.BasePrice,
                p.IsActive,
                p.CreatedAt))
            .FirstOrDefaultAsync(ct);

        if (product is null)
            return Result.Failure<ProductDto>($"Product {query.ProductId} not found.");

        return Result.Success(product);
    }
}
