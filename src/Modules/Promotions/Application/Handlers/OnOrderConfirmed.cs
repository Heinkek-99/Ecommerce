using Ecommerce.Promotions.Infrastructure;
using Ecommerce.Shared.Events;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Promotions.Application.Handlers;

/// <summary>
/// Inbox handler — idempotent par nature.
/// Wolverine garantit la déduplication via incoming_envelopes.
/// </summary>
public class OnOrderConfirmedRecordPromoUse
{
    private readonly PromotionsDbContext _db;

    public OnOrderConfirmedRecordPromoUse(PromotionsDbContext db)
    {
        _db = db;
    }

    public async Task Handle(OrderConfirmedEvent evt, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(evt.PromoCode)) return;

        var promo = await _db.Promotions
            .FirstOrDefaultAsync(p => p.Code == evt.PromoCode, ct);
        if (promo is null) return;

        // Idempotence manuelle : vérifie si cet order_id a déjà été enregistré
        var alreadyUsed = await _db.PromotionUses
            .AnyAsync(u => u.OrderId == evt.OrderId, ct);
        if (alreadyUsed) return;

        promo.RecordUse(evt.UserId, evt.OrderId);
        await _db.SaveChangesAsync(ct);
    }
}
