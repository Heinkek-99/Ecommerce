using Ecommerce.Catalog.Infrastructure;
using Ecommerce.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Catalog.Application.Queries.GetCategoriesTree;

public class GetCategoriesTreeHandler
{
    private readonly CatalogDbContext _db;

    public GetCategoriesTreeHandler(CatalogDbContext db) => _db = db;

    public async Task<Result<List<CategoryTreeDto>>> Handle(
        GetCategoriesTreeQuery query,
        CancellationToken ct)
    {
        var all = await _db.Categories
            .AsNoTracking()
            .Select(c => new { c.Id, c.ParentId, c.Name, c.Slug })
            .ToListAsync(ct);

        // Build tree in-memory (max 3 levels)
        var map = all.ToDictionary(c => c.Id);

        List<CategoryTreeDto> BuildChildren(Guid? parentId, int depth)
        {
            if (depth > 3) return [];
            return all
                .Where(c => c.ParentId == parentId)
                .Select(c => new CategoryTreeDto(
                    c.Id, c.Name, c.Slug,
                    BuildChildren(c.Id, depth + 1)))
                .ToList();
        }

        var tree = BuildChildren(null, 1);
        return Result.Success(tree);
    }
}
