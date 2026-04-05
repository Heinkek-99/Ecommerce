using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Loyalty.Application.Queries.GetLoyaltyDashboard;

public record GetLoyaltyDashboardQuery(Guid UserId) : IQuery<Result<LoyaltyDashboardDto>>;

public record LoyaltyDashboardDto(
    Guid AccountId,
    int PointsBalance,
    int PointsLifetime,
    string CurrentTierName,
    decimal BonusMultiplier,
    int? NextTierThreshold,
    int PointsToNextTier,
    decimal ProgressPercent,
    List<RecentTransactionDto> RecentTransactions);

public record RecentTransactionDto(
    DateTime Date,
    string Type,
    int Points,
    string? Description);
