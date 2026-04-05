using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;
using Microsoft.Extensions.Options;
using Stripe;

namespace Ecommerce.Infrastructure.Stripe;

public class StripeConnectService : IStripeConnectService
{
    private readonly StripeSettings _settings;

    public StripeConnectService(IOptions<StripeSettings> options)
    {
        _settings = options.Value;
    }

    public async Task<Result<string>> CreateConnectedAccountAsync(
        string email,
        string businessName,
        CancellationToken ct)
    {
        try
        {
            var service = new AccountService(new StripeClient(_settings.SecretKey));

            var options = new AccountCreateOptions
            {
                Type = "express",
                Email = email,
                BusinessProfile = new AccountBusinessProfileOptions
                {
                    Name = businessName
                },
                Capabilities = new AccountCapabilitiesOptions
                {
                    CardPayments = new AccountCapabilitiesCardPaymentsOptions
                    {
                        Requested = true
                    },
                    Transfers = new AccountCapabilitiesTransfersOptions
                    {
                        Requested = true
                    }
                }
            };

            var account = await service.CreateAsync(options, cancellationToken: ct);
            return Result.Success(account.Id);
        }
        catch (StripeException ex)
        {
            return Result.Failure<string>($"Stripe error: {ex.StripeError?.Message ?? ex.Message}");
        }
    }

    public async Task<Result<string>> CreateAccountLinkAsync(
        string stripeAccountId,
        string refreshUrl,
        string returnUrl,
        CancellationToken ct)
    {
        try
        {
            var service = new AccountLinkService(new StripeClient(_settings.SecretKey));

            var options = new AccountLinkCreateOptions
            {
                Account = stripeAccountId,
                RefreshUrl = refreshUrl,
                ReturnUrl = returnUrl,
                Type = "account_onboarding"
            };

            var link = await service.CreateAsync(options, cancellationToken: ct);
            return Result.Success(link.Url);
        }
        catch (StripeException ex)
        {
            return Result.Failure<string>($"Stripe error: {ex.StripeError?.Message ?? ex.Message}");
        }
    }

    public async Task<Result<StripeAccountStatus>> GetAccountStatusAsync(
        string stripeAccountId,
        CancellationToken ct)
    {
        try
        {
            var service = new AccountService(new StripeClient(_settings.SecretKey));
            var account = await service.GetAsync(stripeAccountId, cancellationToken: ct);

            return Result.Success(new StripeAccountStatus(
                account.Id,
                account.ChargesEnabled,
                account.PayoutsEnabled));
        }
        catch (StripeException ex)
        {
            return Result.Failure<StripeAccountStatus>($"Stripe error: {ex.StripeError?.Message ?? ex.Message}");
        }
    }
}
