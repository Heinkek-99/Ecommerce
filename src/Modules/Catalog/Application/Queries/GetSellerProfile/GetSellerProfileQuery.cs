using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Catalog.Application.Queries.GetSellerProfile;

public record GetSellerProfileQuery(Guid UserId) : IQuery<Result<SellerProfileDto>>;

public record SellerProfileDto(
    Guid Id,
    Guid UserId,
    string BusinessName,
    string Email,
    string? Phone,
    string? Address,
    string? StripeAccountId,
    bool IsVerified,
    DateTime CreatedAt);
