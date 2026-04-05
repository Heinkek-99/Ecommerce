using Ecommerce.Catalog.Application.Queries.ListProducts;
using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Catalog.Application.Queries.GetCategoryProducts;

public record GetCategoryProductsQuery(
    string CategorySlug,
    string? Search,
    decimal? MinPrice,
    decimal? MaxPrice,
    Guid? SellerId,
    string? SortBy,
    int Page,
    int PageSize = 20
) : IQuery<Result<PagedList<ProductListDto>>>;
