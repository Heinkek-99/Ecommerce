using Ecommerce.Catalog.Domain;
using Ecommerce.Catalog.Infrastructure;
using Ecommerce.Shared.Common;
<<<<<<< HEAD
<<<<<<< HEAD
using Microsoft.EntityFrameworkCore;
=======
using Wolverine;
>>>>>>> feature/orders-logic
=======
using Wolverine;
>>>>>>> feature/notifications-logic

namespace Ecommerce.Catalog.Application.Commands.CreateProduct;

public class CreateProductHandler
{
    private readonly CatalogDbContext _db;

<<<<<<< HEAD
<<<<<<< HEAD
    public CreateProductHandler(CatalogDbContext db) => _db = db;
=======
=======
>>>>>>> feature/notifications-logic
    public CreateProductHandler(CatalogDbContext db)
    {
        _db = db;
    }
<<<<<<< HEAD
>>>>>>> feature/orders-logic
=======
>>>>>>> feature/notifications-logic

    public async Task<Result<Guid>> Handle(
        CreateProductCommand cmd,
        CancellationToken ct)
    {
        var nameGuard = Guard.NotEmpty(cmd.Name, nameof(cmd.Name));
        if (nameGuard.IsFailure) return Result.Failure<Guid>(nameGuard.Error!);

<<<<<<< HEAD
<<<<<<< HEAD
        var slugGuard = Guard.NotEmpty(cmd.Slug, nameof(cmd.Slug));
        if (slugGuard.IsFailure) return Result.Failure<Guid>(slugGuard.Error!);

        var priceGuard = Guard.GreaterThan(cmd.BasePrice, -1m, nameof(cmd.BasePrice));
        if (priceGuard.IsFailure) return Result.Failure<Guid>(priceGuard.Error!);

        var slugExists = await _db.Products
            .AnyAsync(p => p.Slug == cmd.Slug, ct);
        if (slugExists)
            return Result.Failure<Guid>($"Slug '{cmd.Slug}' is already taken.");

        var product = Product.Create(
            cmd.SellerId,
            cmd.SellerName,
            cmd.CategoryId,
            cmd.Name,
            cmd.Slug,
            cmd.Description,
            cmd.BasePrice,
            cmd.MainImageUrl);
=======
=======
>>>>>>> feature/notifications-logic
        var priceGuard = Guard.GreaterThan(cmd.BasePrice, -1m, nameof(cmd.BasePrice));
        if (priceGuard.IsFailure) return Result.Failure<Guid>(priceGuard.Error!);

        var product = Product.Create(
            cmd.SellerId,
            cmd.CategoryId,
            cmd.Name,
            cmd.Description,
            cmd.BasePrice);
<<<<<<< HEAD
>>>>>>> feature/orders-logic
=======
>>>>>>> feature/notifications-logic

        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);

        return Result.Success(product.Id);
    }
}
