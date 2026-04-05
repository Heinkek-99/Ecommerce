using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Catalog.Application.Queries.GetProduct;

public record GetProductQuery(Guid ProductId) : IQuery<Result<ProductDto>>;

public record ProductDto(
    Guid Id,
    Guid SellerId,
    Guid? CategoryId,
    string Name,
    string? Description,
    decimal BasePrice,
    bool IsActive,
    DateTime CreatedAt);
