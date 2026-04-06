using Ecommerce.Loyalty.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Loyalty.Application.Jobs;

/// <summary>
/// Job récurrent (Wolverine Scheduler — remplace Hangfire).
/// Expire les points arrivés à échéance.
/// </summary>
public class ExpireLoyaltyPointsJob
{
    private readonly LoyaltyDbContext _db;

    public ExpireLoyaltyPointsJob(LoyaltyDbContext db)
    {
        _db = db;
    }

    public async Task Execute(CancellationToken ct)
    {
        var expiredTx = await _db.LoyaltyTransactions
            .Where(t => t.Type == "earn"
                && t.ExpiresAt != null
                && t.ExpiresAt <= DateTime.UtcNow)
            .GroupBy(t => t.AccountId)
            .Select(g => new { AccountId = g.Key, Points = g.Sum(t => t.Points) })
            .ToListAsync(ct);

        foreach (var group in expiredTx)
        {
            var account = await _db.LoyaltyAccounts.FindAsync([group.AccountId], ct);
            if (account is null) continue;

            var toExpire = Math.Min(group.Points, account.PointsBalance);
            if (toExpire <= 0) continue;

            // Enregistrer la transaction d'expiration (immuable — pas de UPDATE)
            _db.LoyaltyTransactions.Add(
                Domain.LoyaltyTransaction.Create(
                    group.AccountId,
                    "expire",
                    -toExpire,
                    description: "Expiration automatique des points"));
        }

        if (expiredTx.Any())
            await _db.SaveChangesAsync(ct);
    }
}
