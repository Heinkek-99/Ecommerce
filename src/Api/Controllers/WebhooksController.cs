using Ecommerce.Catalog.Application.Commands.UpdateSellerVerification;
using Ecommerce.Orders.Application.Commands.ConfirmOrderPayment;
using Ecommerce.Orders.Application.Commands.FailOrderPayment;
using Ecommerce.Shared.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Wolverine;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/webhooks")]
[AllowAnonymous]
public class WebhooksController : ControllerBase
{
    private readonly IMessageBus _bus;
    private readonly IConfiguration _config;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(
        IMessageBus bus,
        IConfiguration config,
        ILogger<WebhooksController> logger)
    {
        _bus = bus;
        _config = config;
        _logger = logger;
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> StripeWebhook(CancellationToken ct)
    {
        var webhookSecret = _config["Stripe:WebhookSecret"]
            ?? throw new InvalidOperationException("Stripe:WebhookSecret is not configured.");

        string json;
        using (var reader = new StreamReader(Request.Body))
            json = await reader.ReadToEndAsync(ct);

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                webhookSecret);
        }
        catch (StripeException ex)
        {
            _logger.LogWarning("Stripe webhook signature verification failed: {Message}", ex.Message);
            return BadRequest("Invalid Stripe signature.");
        }

        _logger.LogInformation("Stripe webhook received: {EventType} / {EventId}", stripeEvent.Type, stripeEvent.Id);

        switch (stripeEvent.Type)
        {
            case "payment_intent.succeeded":
                await HandlePaymentSucceededAsync(stripeEvent, ct);
                break;

            case "payment_intent.payment_failed":
                await HandlePaymentFailedAsync(stripeEvent, ct);
                break;

            case "account.updated":
                await HandleAccountUpdatedAsync(stripeEvent, ct);
                break;

            default:
                _logger.LogDebug("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                break;
        }

        // Always return 200 so Stripe does not retry
        return Ok();
    }

    private async Task HandlePaymentSucceededAsync(Event stripeEvent, CancellationToken ct)
    {
        if (stripeEvent.Data.Object is not PaymentIntent intent)
        {
            _logger.LogWarning("payment_intent.succeeded: could not deserialize PaymentIntent.");
            return;
        }

        if (!intent.Metadata.TryGetValue("orderId", out var orderIdStr)
            || !Guid.TryParse(orderIdStr, out var orderId))
        {
            _logger.LogWarning("payment_intent.succeeded: orderId missing from metadata.");
            return;
        }

        var result = await _bus.InvokeAsync<Result>(
            new ConfirmOrderPaymentCommand(orderId, intent.Id), ct);

        if (result.IsFailure)
            _logger.LogError("ConfirmOrderPayment failed for order {OrderId}: {Error}", orderId, result.Error);
    }

    private async Task HandlePaymentFailedAsync(Event stripeEvent, CancellationToken ct)
    {
        if (stripeEvent.Data.Object is not PaymentIntent intent)
        {
            _logger.LogWarning("payment_intent.payment_failed: could not deserialize PaymentIntent.");
            return;
        }

        if (!intent.Metadata.TryGetValue("orderId", out var orderIdStr)
            || !Guid.TryParse(orderIdStr, out var orderId))
        {
            _logger.LogWarning("payment_intent.payment_failed: orderId missing from metadata.");
            return;
        }

        var result = await _bus.InvokeAsync<Result>(
            new FailOrderPaymentCommand(orderId, intent.Id), ct);

        if (result.IsFailure)
            _logger.LogError("FailOrderPayment failed for order {OrderId}: {Error}", orderId, result.Error);
    }

    private async Task HandleAccountUpdatedAsync(Event stripeEvent, CancellationToken ct)
    {
        if (stripeEvent.Data.Object is not Account account)
        {
            _logger.LogWarning("account.updated: could not deserialize Account.");
            return;
        }

        var result = await _bus.InvokeAsync<Result>(
            new UpdateSellerVerificationCommand(
                account.Id,
                account.ChargesEnabled,
                account.PayoutsEnabled), ct);

        if (result.IsFailure)
            _logger.LogError("UpdateSellerVerification failed for account {AccountId}: {Error}",
                account.Id, result.Error);
    }
}
