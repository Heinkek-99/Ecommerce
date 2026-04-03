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

    public LoyaltyController(IMessageBus bus)
    {
        _bus = bus;
    }

    [HttpGet("dashboard")]
    public IActionResult GetDashboard() => Ok(new { message = "Loyalty dashboard" });
}
