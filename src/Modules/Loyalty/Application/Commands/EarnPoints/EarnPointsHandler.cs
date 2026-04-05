using Ecommerce.Loyalty.Infrastructure;
using Ecommerce.Shared.Common;
using Ecommerce.Shared.Events;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Ecommerce.Loyalty.Application.Commands.EarnPoints;

public class EarnPointsHandler
{
    private readonly LoyaltyDbContext _db;

    public EarnPointsHandler(LoyaltyDbContext db) => _db = db;

    public async Task<Result> Handle(EarnPointsCommand cmd, IMessageBus bus, CancellationToken ct)
    {
        // Idempotence : si cet OrderId a déjà été traité, ignorer
        var alreadyProcessed = await _db.LoyaltyTransactions
            .AnyAsync(t => t.OrderId == cmd.OrderId && t.Type == "earn", ct);
        if (alreadyProcessed)
            return Result.Success();

        var account = await _db.LoyaltyAccounts
            .Include(a => a.CurrentTier)
            .FirstOrDefaultAsync(a => a.UserId == cmd.UserId, ct);
        if (account is null)
            return Result.Failure("Loyalty account not found.");

        var program = await _db.LoyaltyPrograms
            .FirstOrDefaultAsync(p => p.IsActive, ct);
        if (program is null)
            return Result.Failure("No active loyalty program.");

        var multiplier = account.CurrentTier?.BonusMultiplier ?? 1m;
        var points = (int)Math.Floor(cmd.OrderAmount * program.PointsPerEuro * multiplier);
        if (points <= 0)
            return Result.Success();

        account.AddPoints(points, cmd.OrderId, $"Commande {cmd.OrderId}");

        // Vérifier upgrade de tier après accumulation
        var allTiers = await _db.LoyaltyTiers
            .Where(t => t.ProgramId == program.Id)
            .OrderByDescending(t => t.MinPoints)
            .ToListAsync(ct);

        var qualifyingTier = allTiers.FirstOrDefault(t => account.PointsLifetime >= t.MinPoints);
        var currentRank = account.CurrentTier?.Rank ?? 0;

        if (qualifyingTier is not null && qualifyingTier.Rank > currentRank)
        {
            var oldTierName = account.CurrentTier?.Name ?? "Bronze";
            account.UpgradeTier(qualifyingTier.Id, qualifyingTier.Name);

            // Outbox : AVANT SaveChanges
            await bus.PublishAsync(new TierUpgradedEvent(
                cmd.UserId,
                oldTierName,
                qualifyingTier.Name,
                DateTime.UtcNow));
        }

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
