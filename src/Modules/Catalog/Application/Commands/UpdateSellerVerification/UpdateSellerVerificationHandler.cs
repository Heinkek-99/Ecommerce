using Ecommerce.Catalog.Infrastructure;
using Ecommerce.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Catalog.Application.Commands.UpdateSellerVerification;

public class UpdateSellerVerificationHandler
{
    private readonly CatalogDbContext _db;

    public UpdateSellerVerificationHandler(CatalogDbContext db) => _db = db;

    public async Task<Result> Handle(
        UpdateSellerVerificationCommand cmd,
        CancellationToken ct)
    {
        var seller = await _db.Sellers
            .FirstOrDefaultAsync(s => s.StripeAccountId == cmd.StripeAccountId, ct);

        if (seller is null)
            // Account may not be registered yet — treat as no-op
            return Result.Success();

        seller.UpdateVerification(cmd.ChargesEnabled && cmd.PayoutsEnabled);
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
