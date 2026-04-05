using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Catalog.Application.Queries.GetStripeOnboarding;

public record GetStripeOnboardingQuery(
    Guid UserId,
    string RefreshUrl,
    string ReturnUrl
) : IQuery<Result<string>>;
