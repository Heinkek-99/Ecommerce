using System.Security.Claims;
using Ecommerce.Catalog.Application.Commands.RegisterSeller;
using Ecommerce.Catalog.Application.Queries.GetSellerProfile;
using Ecommerce.Catalog.Application.Queries.GetStripeOnboarding;
using Ecommerce.Shared.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Ecommerce.Catalog.Api;

[ApiController]
[Route("api/sellers")]
[Authorize]
public class SellersController : ControllerBase
{
    private readonly IMessageBus _bus;

    public SellersController(IMessageBus bus) => _bus = bus;

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");
        return Guid.Parse(claim!);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterSellerRequest req)
    {
        var cmd = new RegisterSellerCommand(GetUserId(), req.BusinessName, req.Email, req.Phone, req.Address);
        var result = await _bus.InvokeAsync<Result<Guid>>(cmd);
        return result.IsSuccess ? Ok(new { sellerId = result.Value }) : BadRequest(result.Error);
    }

    [HttpGet("stripe/onboarding")]
    public async Task<IActionResult> GetStripeOnboarding(
        [FromQuery] string refreshUrl,
        [FromQuery] string returnUrl)
    {
        var query = new GetStripeOnboardingQuery(GetUserId(), refreshUrl, returnUrl);
        var result = await _bus.InvokeAsync<Result<string>>(query);
        return result.IsSuccess ? Ok(new { url = result.Value }) : BadRequest(result.Error);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _bus.InvokeAsync<Result<SellerProfileDto>>(new GetSellerProfileQuery(GetUserId()));
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }
}

public record RegisterSellerRequest(
    string BusinessName,
    string Email,
    string? Phone = null,
    string? Address = null);
