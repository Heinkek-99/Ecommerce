using Ecommerce.Shared.Common;

namespace Ecommerce.Shared.Abstractions;

public interface IStripeConnectService
{
    Task<Result<string>> CreateConnectedAccountAsync(
        string email,
        string businessName,
        CancellationToken ct);

    Task<Result<string>> CreateAccountLinkAsync(
        string stripeAccountId,
        string refreshUrl,
        string returnUrl,
        CancellationToken ct);

    Task<Result<StripeAccountStatus>> GetAccountStatusAsync(
        string stripeAccountId,
        CancellationToken ct);
}
