using Ecommerce.Catalog.Domain;
using Ecommerce.Catalog.Infrastructure;
using Ecommerce.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Catalog.Application.Commands.CreateProduct;

public class CreateProductHandler
{
    private readonly CatalogDbContext _db;

    public CreateProductHandler(CatalogDbContext db) => _db = db;

    public async Task<Result<Guid>> Handle(
        CreateProductCommand cmd,
        CancellationToken ct)
    {
        var nameGuard = Guard.NotEmpty(cmd.Name, nameof(cmd.Name));
        if (nameGuard.IsFailure) return Result.Failure<Guid>(nameGuard.Error!);

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

        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);

        return Result.Success(product.Id);
    }
}
