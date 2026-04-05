using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Catalog.Application.Commands.CreateProduct;

public record CreateProductCommand(
    Guid SellerId,
<<<<<<< HEAD
<<<<<<< HEAD
    string SellerName,
    Guid? CategoryId,
    string Name,
    string Slug,
    string? Description,
    decimal BasePrice,
    string? MainImageUrl = null
=======
=======
>>>>>>> feature/orders-logic
    Guid? CategoryId,
    string Name,
    string? Description,
    decimal BasePrice
<<<<<<< HEAD
>>>>>>> feature/identity-auth
=======
>>>>>>> feature/orders-logic
) : ICommand<Result<Guid>>;
