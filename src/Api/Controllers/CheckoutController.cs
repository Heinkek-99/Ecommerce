using System.Security.Claims;
using Ecommerce.Cart.Application.Commands.ClearCart;
using Ecommerce.Cart.Application.Services;
using Ecommerce.Catalog.Infrastructure;
using Ecommerce.Orders.Application.Commands.Checkout;
using Ecommerce.Shared.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class CheckoutController : ControllerBase
{
    private readonly IMessageBus _bus;
    private readonly ICartStorage _cartStorage;
    private readonly CatalogDbContext _catalogDb;

    public CheckoutController(IMessageBus bus, ICartStorage cartStorage, CatalogDbContext catalogDb)
    {
        _bus = bus;
        _cartStorage = cartStorage;
        _catalogDb = catalogDb;
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");
        return Guid.Parse(claim!);
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(
        [FromBody] CheckoutRequest req,
        CancellationToken ct)
    {
        var userId = GetUserId();
        var cart = await _cartStorage.GetAsync(userId, ct);

        if (cart is null || cart.Items.Count == 0)
            return BadRequest("Cart is empty.");

        // Résout le StripeAccountId du premier vendeur (MVP : un vendeur par commande)
        var firstSellerId = cart.Items.First().SellerId;
        var sellerStripeAccountId = await _catalogDb.Sellers
            .AsNoTracking()
            .Where(s => s.UserId == firstSellerId)
            .Select(s => s.StripeAccountId)
            .FirstOrDefaultAsync(ct);

        var items = cart.Items.Select(i => new CheckoutItemLine(
            i.VariantId,
            i.ProductId,
            i.SellerId,
            i.ProductName,
            i.Sku,
            i.UnitPrice,
            i.Quantity)).ToList();

        var cmd = new CheckoutCommand(
            userId,
            req.AddressId,
            req.CurrencyCode ?? "EUR",
            items,
            sellerStripeAccountId);

        var result = await _bus.InvokeAsync<Result<CheckoutResult>>(cmd, ct);

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        // Vide le panier après checkout réussi
        await _bus.InvokeAsync<Result>(new ClearCartCommand(userId), ct);

        return Ok(new
        {
            orderId = result.Value.OrderId,
            clientSecret = result.Value.ClientSecret
        });
    }
}

public record CheckoutRequest(Guid AddressId, string? CurrencyCode = "EUR");
