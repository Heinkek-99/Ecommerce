using Ecommerce.Catalog.Infrastructure;
using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Catalog.Application.Queries.GetStripeOnboarding;

public class GetStripeOnboardingHandler
{
    private readonly CatalogDbContext _db;
    private readonly IStripeConnectService _stripe;

    public GetStripeOnboardingHandler(CatalogDbContext db, IStripeConnectService stripe)
    {
        _db = db;
        _stripe = stripe;
    }

    public async Task<Result<string>> Handle(GetStripeOnboardingQuery query, CancellationToken ct)
    {
        var seller = await _db.Sellers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == query.UserId, ct);

        if (seller is null)
            return Result.Failure<string>("Seller account not found.");

        if (seller.StripeAccountId is null)
            return Result.Failure<string>("No Stripe account connected. Register first.");

        return await _stripe.CreateAccountLinkAsync(
            seller.StripeAccountId,
            query.RefreshUrl,
            query.ReturnUrl,
            ct);
    }
}
