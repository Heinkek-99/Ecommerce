using Ecommerce.Shared.Common;

namespace Ecommerce.Shared.Abstractions;

public interface IStripePaymentService
{
    Task<Result<PaymentIntentResult>> CreatePaymentIntentAsync(
        decimal amount,
        string currency,
        string sellerStripeAccountId,
        decimal commissionRate,
        Dictionary<string, string> metadata,
        CancellationToken ct);

    Task<Result> RefundPaymentAsync(string paymentIntentId, CancellationToken ct);
}
