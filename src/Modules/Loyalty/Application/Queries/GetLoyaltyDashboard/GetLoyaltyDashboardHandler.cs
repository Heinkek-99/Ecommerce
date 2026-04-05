using Ecommerce.Loyalty.Domain;
using Ecommerce.Loyalty.Infrastructure;
using Ecommerce.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Loyalty.Application.Queries.GetLoyaltyDashboard;

public class GetLoyaltyDashboardHandler
{
    private readonly LoyaltyDbContext _db;

    public GetLoyaltyDashboardHandler(LoyaltyDbContext db) => _db = db;

    public async Task<Result<LoyaltyDashboardDto>> Handle(
        GetLoyaltyDashboardQuery query,
        CancellationToken ct)
    {
        var account = await _db.LoyaltyAccounts
            .AsNoTracking()
            .Include(a => a.CurrentTier)
            .FirstOrDefaultAsync(a => a.UserId == query.UserId, ct);

        if (account is null)
            return Result.Failure<LoyaltyDashboardDto>("Loyalty account not found.");

        var recentTransactions = await _db.LoyaltyTransactions
            .AsNoTracking()
            .Where(t => t.AccountId == account.Id)
            .OrderByDescending(t => t.CreatedAt)
            .Take(10)
            .Select(t => new RecentTransactionDto(t.CreatedAt, t.Type, t.Points, t.Description))
            .ToListAsync(ct);

        // Trouver le tier suivant
        LoyaltyTier? nextTier = null;
        if (account.CurrentTier is not null)
        {
            nextTier = await _db.LoyaltyTiers
                .AsNoTracking()
                .Where(t => t.ProgramId == account.CurrentTier.ProgramId
                         && t.Rank > account.CurrentTier.Rank)
                .OrderBy(t => t.Rank)
                .FirstOrDefaultAsync(ct);
        }

        var currentMin = account.CurrentTier?.MinPoints ?? 0;
        var nextThreshold = nextTier?.MinPoints;
        var pointsToNext = nextThreshold.HasValue
            ? Math.Max(0, nextThreshold.Value - account.PointsLifetime)
            : 0;

        decimal progressPercent = 0m;
        if (nextThreshold.HasValue && nextThreshold.Value > currentMin)
        {
            var earned = account.PointsLifetime - currentMin;
            var needed = nextThreshold.Value - currentMin;
            progressPercent = Math.Min(100m, Math.Round((decimal)earned / needed * 100m, 1));
        }

        var dto = new LoyaltyDashboardDto(
            AccountId: account.Id,
            PointsBalance: account.PointsBalance,
            PointsLifetime: account.PointsLifetime,
            CurrentTierName: account.CurrentTier?.Name ?? "Bronze",
            BonusMultiplier: account.CurrentTier?.BonusMultiplier ?? 1m,
            NextTierThreshold: nextThreshold,
            PointsToNextTier: pointsToNext,
            ProgressPercent: progressPercent,
            RecentTransactions: recentTransactions);

        return Result.Success(dto);
    }
}
