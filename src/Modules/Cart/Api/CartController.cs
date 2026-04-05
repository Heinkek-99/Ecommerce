using System.Security.Claims;
using Ecommerce.Cart.Application.Commands.AddToCart;
using Ecommerce.Cart.Application.Commands.ClearCart;
using Ecommerce.Cart.Application.Commands.RemoveFromCart;
using Ecommerce.Cart.Application.Commands.UpdateCartItem;
using Ecommerce.Cart.Application.Queries.GetCart;
using Ecommerce.Shared.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Ecommerce.Cart.Api;

[ApiController]
[Route("api/cart")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly IMessageBus _bus;

    public CartController(IMessageBus bus) => _bus = bus;

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");
        return Guid.Parse(claim!);
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var result = await _bus.InvokeAsync<Result<ShoppingCartDto>>(new GetCartQuery(GetUserId()));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddItemRequest request)
    {
        var result = await _bus.InvokeAsync<Result>(
            new AddToCartCommand(GetUserId(), request.VariantId, request.Quantity));
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpPut("items/{variantId:guid}")]
    public async Task<IActionResult> UpdateItem(Guid variantId, [FromBody] UpdateItemRequest request)
    {
        var result = await _bus.InvokeAsync<Result>(
            new UpdateCartItemCommand(GetUserId(), variantId, request.NewQuantity));
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpDelete("items/{variantId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid variantId)
    {
        var result = await _bus.InvokeAsync<Result>(
            new RemoveFromCartCommand(GetUserId(), variantId));
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var result = await _bus.InvokeAsync<Result>(new ClearCartCommand(GetUserId()));
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }
}

public record AddItemRequest(Guid VariantId, int Quantity);
public record UpdateItemRequest(int NewQuantity);
