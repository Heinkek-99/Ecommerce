using System.Security.Claims;
using Ecommerce.Orders.Application.Commands.PrepareOrder;
using Ecommerce.Orders.Application.Commands.ShipOrder;
using Ecommerce.Orders.Application.Queries.GetSellerOrderDetail;
using Ecommerce.Orders.Application.Queries.GetSellerOrders;
using Ecommerce.Shared.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Ecommerce.Orders.Api;

[ApiController]
[Route("api/orders/seller")]
[Authorize]
public class SellerOrdersController : ControllerBase
{
    private readonly IMessageBus _bus;

    public SellerOrdersController(IMessageBus bus) => _bus = bus;

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");
        return Guid.Parse(claim!);
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _bus.InvokeAsync<Result<PagedList<SellerOrderDto>>>(
            new GetSellerOrdersQuery(GetUserId(), status, page, pageSize));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> GetOrderDetail(Guid orderId)
    {
        var result = await _bus.InvokeAsync<Result<SellerOrderDetailDto>>(
            new GetSellerOrderDetailQuery(orderId, GetUserId()));
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPut("{orderId:guid}/prepare")]
    public async Task<IActionResult> Prepare(Guid orderId)
    {
        var result = await _bus.InvokeAsync<Result>(new PrepareOrderCommand(orderId, GetUserId()));
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpPut("{orderId:guid}/ship")]
    public async Task<IActionResult> Ship(Guid orderId, [FromBody] ShipRequest req)
    {
        var result = await _bus.InvokeAsync<Result>(
            new ShipOrderCommand(orderId, GetUserId(), req.TrackingNumber, req.Carrier));
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }
}

public record ShipRequest(string TrackingNumber, string Carrier);
