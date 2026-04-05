using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Catalog.Application.Queries.ListProducts;

public record ListProductsQuery(
    string? Search,
    string? CategorySlug,
    decimal? MinPrice,
    decimal? MaxPrice,
    Guid? SellerId,
    string? SortBy,
    int Page,
    int PageSize = 20
) : IQuery<Result<PagedList<ProductListDto>>>;

public record ProductListDto(
    Guid Id,
    string Name,
    string Slug,
    decimal BasePrice,
    string? MainImageUrl,
    string SellerName,
    string? CategoryName);
