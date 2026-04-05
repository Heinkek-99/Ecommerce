using Ecommerce.Catalog.Domain;
using Ecommerce.Catalog.Infrastructure;
using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Catalog.Application.Commands.RegisterSeller;

public class RegisterSellerHandler
{
    private readonly CatalogDbContext _db;
    private readonly IStripeConnectService _stripe;
    private readonly IIdentityIntegrationService _identity;

    public RegisterSellerHandler(
        CatalogDbContext db,
        IStripeConnectService stripe,
        IIdentityIntegrationService identity)
    {
        _db = db;
        _stripe = stripe;
        _identity = identity;
    }

    public async Task<Result<Guid>> Handle(RegisterSellerCommand cmd, CancellationToken ct)
    {
        var existing = await _db.Sellers
            .AnyAsync(s => s.UserId == cmd.UserId, ct);
        if (existing)
            return Result.Failure<Guid>("Seller account already exists for this user.");

        var seller = Seller.Create(cmd.UserId, cmd.BusinessName, cmd.Email, cmd.Phone, cmd.Address);
        _db.Sellers.Add(seller);
        await _db.SaveChangesAsync(ct);

        // Ajoute le rôle "seller" au user dans Identity (synchrone, cross-module via interface)
        await _identity.AddRoleAsync(cmd.UserId, "seller", ct);

        // Connect Stripe after persisting (non-critical — can retry via onboarding endpoint)
        var stripeResult = await _stripe.CreateConnectedAccountAsync(cmd.Email, cmd.BusinessName, ct);
        if (stripeResult.IsSuccess)
        {
            seller.ConnectStripe(stripeResult.Value);
            await _db.SaveChangesAsync(ct);
        }

        return Result.Success(seller.Id);
    }
}
