namespace Ecommerce.Shared.Common;

public record PaymentIntentResult(
    string PaymentIntentId,
    string ClientSecret,
    string Status);
