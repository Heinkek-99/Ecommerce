using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Catalog.Application.Commands.UpdateSellerVerification;

public record UpdateSellerVerificationCommand(
    string StripeAccountId,
    bool ChargesEnabled,
    bool PayoutsEnabled
) : ICommand<Result>;
