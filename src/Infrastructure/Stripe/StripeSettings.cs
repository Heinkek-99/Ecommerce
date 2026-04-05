namespace Ecommerce.Infrastructure.Stripe;

public record StripeSettings(
    string SecretKey,
    string PublishableKey,
    string WebhookSecret,
    decimal CommissionRate);
