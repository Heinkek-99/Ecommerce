using Ecommerce.Catalog.Infrastructure;
using Ecommerce.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Catalog.Application.Queries.ListProducts;

public class ListProductsHandler
{
    private readonly CatalogDbContext _db;

    public ListProductsHandler(CatalogDbContext db) => _db = db;

    public async Task<Result<PagedList<ProductListDto>>> Handle(
        ListProductsQuery query,
        CancellationToken ct)
    {
        var q = _db.Products
            .AsNoTracking()
            .Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(p =>
                EF.Functions.ILike(p.Name, $"%{query.Search}%") ||
                EF.Functions.ILike(p.Description ?? "", $"%{query.Search}%"));

        if (!string.IsNullOrWhiteSpace(query.CategorySlug))
        {
            var categoryId = await _db.Categories
                .AsNoTracking()
                .Where(c => c.Slug == query.CategorySlug)
                .Select(c => (Guid?)c.Id)
                .FirstOrDefaultAsync(ct);

            if (categoryId is null)
                return Result.Failure<PagedList<ProductListDto>>($"Category '{query.CategorySlug}' not found.");

            q = q.Where(p => p.CategoryId == categoryId);
        }

        if (query.MinPrice.HasValue)
            q = q.Where(p => p.BasePrice >= query.MinPrice.Value);

        if (query.MaxPrice.HasValue)
            q = q.Where(p => p.BasePrice <= query.MaxPrice.Value);

        if (query.SellerId.HasValue)
            q = q.Where(p => p.SellerId == query.SellerId.Value);

        q = query.SortBy?.ToLowerInvariant() switch
        {
            "price"   => q.OrderBy(p => p.BasePrice),
            "newest"  => q.OrderByDescending(p => p.CreatedAt),
            "popular" => q.OrderByDescending(p => p.CreatedAt), // placeholder — pas de view_count en base
            _         => q.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await q.CountAsync(ct);

        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var page = Math.Max(query.Page, 1);

        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductListDto(
                p.Id,
                p.Name,
                p.Slug,
                p.BasePrice,
                p.MainImageUrl,
                p.SellerName,
                _db.Categories
                    .Where(c => c.Id == p.CategoryId)
                    .Select(c => c.Name)
                    .FirstOrDefault()))
            .ToListAsync(ct);

        return Result.Success(new PagedList<ProductListDto>(items, totalCount, page, pageSize));
    }
}
