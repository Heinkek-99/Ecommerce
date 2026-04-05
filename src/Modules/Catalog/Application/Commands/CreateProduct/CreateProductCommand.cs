using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Catalog.Application.Commands.CreateProduct;

public record CreateProductCommand(
    Guid SellerId,
    string SellerName,
    Guid? CategoryId,
    string Name,
    string Slug,
    string? Description,
    decimal BasePrice,
    string? MainImageUrl = null
) : ICommand<Result<Guid>>;
