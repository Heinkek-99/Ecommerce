using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;
using Microsoft.Extensions.Options;
using Stripe;

namespace Ecommerce.Infrastructure.Stripe;

public class StripePaymentService : IStripePaymentService
{
    private readonly StripeSettings _settings;

    public StripePaymentService(IOptions<StripeSettings> options)
    {
        _settings = options.Value;
    }

    public async Task<Result<PaymentIntentResult>> CreatePaymentIntentAsync(
        decimal amount,
        string currency,
        string sellerStripeAccountId,
        decimal commissionRate,
        Dictionary<string, string> metadata,
        CancellationToken ct)
    {
        try
        {
            var service = new PaymentIntentService(new StripeClient(_settings.SecretKey));

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100),
                Currency = currency.ToLowerInvariant(),
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                },
                ApplicationFeeAmount = (long)(amount * commissionRate * 100),
                TransferData = new PaymentIntentTransferDataOptions
                {
                    Destination = sellerStripeAccountId
                },
                Metadata = metadata
            };

            var intent = await service.CreateAsync(options, cancellationToken: ct);

            return Result.Success(new PaymentIntentResult(
                intent.Id,
                intent.ClientSecret,
                intent.Status));
        }
        catch (StripeException ex)
        {
            return Result.Failure<PaymentIntentResult>($"Stripe error: {ex.StripeError?.Message ?? ex.Message}");
        }
    }

    public async Task<Result> RefundPaymentAsync(string paymentIntentId, CancellationToken ct)
    {
        try
        {
            var service = new RefundService(new StripeClient(_settings.SecretKey));

            var options = new RefundCreateOptions
            {
                PaymentIntent = paymentIntentId
            };

            await service.CreateAsync(options, cancellationToken: ct);
            return Result.Success();
        }
        catch (StripeException ex)
        {
            return Result.Failure($"Stripe refund error: {ex.StripeError?.Message ?? ex.Message}");
        }
    }
}
