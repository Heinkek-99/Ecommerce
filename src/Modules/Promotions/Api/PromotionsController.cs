using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Ecommerce.Promotions.Api;

[ApiController]
[Route("api/promotions")]
[Authorize]
public class PromotionsController : ControllerBase
{
    private readonly IMessageBus _bus;

    public PromotionsController(IMessageBus bus)
    {
        _bus = bus;
    }

    [HttpGet("active")]
    public IActionResult GetActive() => Ok(new { message = "Active promotions endpoint" });
}
