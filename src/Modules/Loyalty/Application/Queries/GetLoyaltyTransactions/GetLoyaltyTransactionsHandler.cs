using Ecommerce.Loyalty.Infrastructure;
using Ecommerce.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Loyalty.Application.Queries.GetLoyaltyTransactions;

public class GetLoyaltyTransactionsHandler
{
    private readonly LoyaltyDbContext _db;

    public GetLoyaltyTransactionsHandler(LoyaltyDbContext db) => _db = db;

    public async Task<Result<PagedList<LoyaltyTransactionDto>>> Handle(
        GetLoyaltyTransactionsQuery query,
        CancellationToken ct)
    {
        var account = await _db.LoyaltyAccounts
            .AsNoTracking()
            .Where(a => a.UserId == query.UserId)
            .Select(a => a.Id)
            .FirstOrDefaultAsync(ct);

        if (account == Guid.Empty)
            return Result.Failure<PagedList<LoyaltyTransactionDto>>("Loyalty account not found.");

        var totalCount = await _db.LoyaltyTransactions
            .AsNoTracking()
            .CountAsync(t => t.AccountId == account, ct);

        var items = await _db.LoyaltyTransactions
            .AsNoTracking()
            .Where(t => t.AccountId == account)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(t => new LoyaltyTransactionDto(t.Id, t.CreatedAt, t.Type, t.Points, t.Description))
            .ToListAsync(ct);

        return Result.Success(new PagedList<LoyaltyTransactionDto>(items, totalCount, query.Page, query.PageSize));
    }
}
