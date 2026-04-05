using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Loyalty.Application.Commands.RedeemReward;

public record RedeemRewardCommand(
    Guid UserId,
    Guid RewardId) : ICommand<Result<Guid>>;
