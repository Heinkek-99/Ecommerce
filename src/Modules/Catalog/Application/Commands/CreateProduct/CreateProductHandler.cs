using Ecommerce.Catalog.Domain;
using Ecommerce.Catalog.Infrastructure;
using Ecommerce.Shared.Common;
using Wolverine;

namespace Ecommerce.Catalog.Application.Commands.CreateProduct;

public class CreateProductHandler
{
    private readonly CatalogDbContext _db;

    public CreateProductHandler(CatalogDbContext db)
    {
        _db = db;
    }

    public async Task<Result<Guid>> Handle(
        CreateProductCommand cmd,
        CancellationToken ct)
    {
        var nameGuard = Guard.NotEmpty(cmd.Name, nameof(cmd.Name));
        if (nameGuard.IsFailure) return Result.Failure<Guid>(nameGuard.Error!);

        var priceGuard = Guard.GreaterThan(cmd.BasePrice, -1m, nameof(cmd.BasePrice));
        if (priceGuard.IsFailure) return Result.Failure<Guid>(priceGuard.Error!);

        var product = Product.Create(
            cmd.SellerId,
            cmd.CategoryId,
            cmd.Name,
            cmd.Description,
            cmd.BasePrice);

        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);

        return Result.Success(product.Id);
    }
}
