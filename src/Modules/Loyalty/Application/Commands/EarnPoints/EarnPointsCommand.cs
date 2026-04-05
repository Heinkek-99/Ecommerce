using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Loyalty.Application.Commands.EarnPoints;

public record EarnPointsCommand(
    Guid UserId,
    Guid OrderId,
    decimal OrderAmount) : ICommand<Result>;
