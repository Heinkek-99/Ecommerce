using Ecommerce.Loyalty.Infrastructure;
using Ecommerce.Shared.Events;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Loyalty.Application.Handlers;

/// <summary>
/// Inbox handler — idempotent par nature.
/// Wolverine garantit la déduplication via incoming_envelopes.
/// Double protection manuelle par vérification order_id.
/// </summary>
public class OnOrderConfirmedCreditPoints
{
    private readonly LoyaltyDbContext _db;

    public OnOrderConfirmedCreditPoints(LoyaltyDbContext db)
    {
        _db = db;
    }

    public async Task Handle(OrderConfirmedEvent evt, CancellationToken ct)
    {
        // Idempotence manuelle : vérifier si cet order_id a déjà généré une transaction
        var alreadyProcessed = await _db.LoyaltyTransactions
            .AnyAsync(t => t.OrderId == evt.OrderId && t.Type == "earn", ct);
        if (alreadyProcessed) return;

        var account = await _db.LoyaltyAccounts
            .Include(a => a.CurrentTier)
            .FirstOrDefaultAsync(a => a.UserId == evt.UserId, ct);
        if (account is null) return;

        var program = await _db.LoyaltyPrograms
            .FirstOrDefaultAsync(p => p.IsActive, ct);
        if (program is null) return;

        var multiplier = account.CurrentTier?.BonusMultiplier ?? 1m;
        var points = (int)Math.Floor(evt.TotalAmount * program.PointsPerEuro * multiplier);

        if (points <= 0) return;

        account.AddPoints(points, evt.OrderId, $"Commande {evt.OrderId}");
        await _db.SaveChangesAsync(ct);
    }
}
