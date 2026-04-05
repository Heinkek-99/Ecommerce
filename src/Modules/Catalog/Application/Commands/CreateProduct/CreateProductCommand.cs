using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Catalog.Application.Commands.CreateProduct;

public record CreateProductCommand(
    Guid SellerId,
    Guid? CategoryId,
    string Name,
    string? Description,
    decimal BasePrice
) : ICommand<Result<Guid>>;
