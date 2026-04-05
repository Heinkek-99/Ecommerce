using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Catalog.Application.Queries.GetProductBySlug;

public record GetProductBySlugQuery(string Slug) : IQuery<Result<ProductDetailDto>>;

public record ProductDetailDto(
    Guid Id,
    Guid SellerId,
    string SellerName,
    string Name,
    string Slug,
    string? Description,
    decimal BasePrice,
    string? MainImageUrl,
    bool IsActive,
    DateTime CreatedAt,
    Guid? CategoryId,
    string? CategoryName,
    List<VariantDto> Variants);

public record VariantDto(
    Guid Id,
    string Sku,
    string? Color,
    string? Size,
    decimal? Price,
    int StockQuantity);
