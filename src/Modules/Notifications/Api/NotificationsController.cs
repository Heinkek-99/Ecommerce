using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Ecommerce.Notifications.Api;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMessageBus _bus;

    public NotificationsController(IMessageBus bus)
    {
        _bus = bus;
    }

    [HttpGet]
    public IActionResult GetNotifications() =>
        Ok(new { message = "Notifications endpoint" });
}
