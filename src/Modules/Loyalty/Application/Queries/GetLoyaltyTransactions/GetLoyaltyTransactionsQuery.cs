using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Loyalty.Application.Queries.GetLoyaltyTransactions;

public record GetLoyaltyTransactionsQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 20) : IQuery<Result<PagedList<LoyaltyTransactionDto>>>;

public record LoyaltyTransactionDto(
    Guid Id,
    DateTime Date,
    string Type,
    int Points,
    string? Description);
