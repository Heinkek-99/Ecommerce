using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Catalog.Application.Commands.CreateProduct;

public record CreateProductCommand(
    Guid SellerId,
<<<<<<< HEAD
    string SellerName,
    Guid? CategoryId,
    string Name,
    string Slug,
    string? Description,
    decimal BasePrice,
    string? MainImageUrl = null
=======
    Guid? CategoryId,
    string Name,
    string? Description,
    decimal BasePrice
>>>>>>> feature/identity-auth
) : ICommand<Result<Guid>>;
