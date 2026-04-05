using Ecommerce.Orders.Domain;
using Ecommerce.Orders.Infrastructure;
using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;

namespace Ecommerce.Orders.Application.Commands.Checkout;

public class CheckoutHandler
{
    private readonly OrdersDbContext _db;
    private readonly ICatalogIntegrationService _catalog;
    private readonly IStripePaymentService _stripe;

    public CheckoutHandler(
        OrdersDbContext db,
        ICatalogIntegrationService catalog,
        IStripePaymentService stripe)
    {
        _db = db;
        _catalog = catalog;
        _stripe = stripe;
    }

    public async Task<Result<CheckoutResult>> Handle(CheckoutCommand cmd, CancellationToken ct)
    {
        if (cmd.Items is null || cmd.Items.Count == 0)
            return Result.Failure<CheckoutResult>("Cart is empty.");

        // Décrémente le stock de manière atomique (valide aussi la disponibilité)
        var stockItems = cmd.Items.Select(i => (i.VariantId, i.Quantity));
        var stockResult = await _catalog.DecrementStockAsync(stockItems, ct);
        if (stockResult.IsFailure)
            return Result.Failure<CheckoutResult>(stockResult.Error!);

        // Crée l'order en statut pending_payment
        var lines = cmd.Items.Select(i => new OrderItemLine(
            i.VariantId, i.ProductId, i.SellerId,
            i.ProductName, i.ProductSku, i.Quantity, i.UnitPrice)).ToList();

        var order = Order.Create(cmd.UserId, cmd.AddressId, cmd.CurrencyCode, lines);
        order.SetPendingPayment();
        _db.Orders.Add(order);

        // Obtenir le sellerId du premier item (MVP : on suppose un vendeur par commande)
        var sellerId = cmd.Items.First().SellerId;
        var commissionRate = 0.10m; // override par Stripe:CommissionRate si nécessaire

        // Obtenir le StripeAccountId du vendeur via Catalog
        // (On l'a dans SellerId — le CheckoutController l'a résolu en amont)
        var sellerStripeAccountId = cmd.SellerStripeAccountId ?? string.Empty;

        var metadata = new Dictionary<string, string>
        {
            ["orderId"]  = order.Id.ToString(),
            ["userId"]   = cmd.UserId.ToString(),
            ["sellerId"] = sellerId.ToString()
        };

        var intentResult = await _stripe.CreatePaymentIntentAsync(
            order.TotalAmount,
            cmd.CurrencyCode,
            sellerStripeAccountId,
            commissionRate,
            metadata,
            ct);

        if (intentResult.IsFailure)
        {
            // Rollback stock (best-effort)
            foreach (var item in cmd.Items)
                await _catalog.DecrementStockAsync([(item.VariantId, -item.Quantity)], ct);

            return Result.Failure<CheckoutResult>($"Payment initialization failed: {intentResult.Error}");
        }

        await _db.SaveChangesAsync(ct);

        return Result.Success(new CheckoutResult(order.Id, intentResult.Value.ClientSecret));
    }
}
