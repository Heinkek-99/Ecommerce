using Ecommerce.Catalog.Infrastructure;
using Ecommerce.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Catalog.Application.Queries.GetSellerProfile;

public class GetSellerProfileHandler
{
    private readonly CatalogDbContext _db;

    public GetSellerProfileHandler(CatalogDbContext db) => _db = db;

    public async Task<Result<SellerProfileDto>> Handle(GetSellerProfileQuery query, CancellationToken ct)
    {
        var seller = await _db.Sellers
            .AsNoTracking()
            .Where(s => s.UserId == query.UserId)
            .Select(s => new SellerProfileDto(
                s.Id, s.UserId, s.BusinessName, s.Email,
                s.Phone, s.Address, s.StripeAccountId, s.IsVerified, s.CreatedAt))
            .FirstOrDefaultAsync(ct);

        if (seller is null)
            return Result.Failure<SellerProfileDto>("Seller profile not found.");

        return Result.Success(seller);
    }
}
