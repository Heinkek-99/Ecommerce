namespace Ecommerce.Shared.Common;

public record StripeAccountStatus(
    string AccountId,
    bool IsVerified,
    bool PayoutsEnabled);
