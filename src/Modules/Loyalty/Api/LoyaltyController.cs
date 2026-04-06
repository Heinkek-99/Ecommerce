<<<<<<< HEAD
using System.Security.Claims;
using Ecommerce.Loyalty.Application.Commands.RedeemReward;
using Ecommerce.Loyalty.Application.Queries.GetLoyaltyDashboard;
using Ecommerce.Loyalty.Application.Queries.GetLoyaltyTransactions;
using Ecommerce.Shared.Common;
=======
>>>>>>> feature/notifications-logic
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Ecommerce.Loyalty.Api;

[ApiController]
[Route("api/loyalty")]
[Authorize]
public class LoyaltyController : ControllerBase
{
    private readonly IMessageBus _bus;

<<<<<<< HEAD
    public LoyaltyController(IMessageBus bus) => _bus = bus;

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");
        return Guid.Parse(claim!);
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
    {
        var result = await _bus.InvokeAsync<Result<LoyaltyDashboardDto>>(
            new GetLoyaltyDashboardQuery(GetUserId()), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPost("redeem")]
    public async Task<IActionResult> Redeem([FromBody] RedeemRewardCommand cmd, CancellationToken ct)
    {
        var command = cmd with { UserId = GetUserId() };
        var result = await _bus.InvokeAsync<Result<Guid>>(command, ct);
        return result.IsSuccess
            ? Ok(new { redemptionId = result.Value })
            : BadRequest(result.Error);
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _bus.InvokeAsync<Result<PagedList<LoyaltyTransactionDto>>>(
            new GetLoyaltyTransactionsQuery(GetUserId(), page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }
=======
    public LoyaltyController(IMessageBus bus)
    {
        _bus = bus;
    }

    [HttpGet("dashboard")]
    public IActionResult GetDashboard() => Ok(new { message = "Loyalty dashboard" });
>>>>>>> feature/notifications-logic
}
