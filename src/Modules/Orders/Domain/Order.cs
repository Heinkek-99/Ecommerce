namespace Ecommerce.Orders.Domain;

public class Order
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid AddressId { get; private set; }
    public string Status { get; private set; } = "pending";
    public decimal TotalAmount { get; private set; }
    public string CurrencyCode { get; private set; } = "EUR";
    public decimal? TotalInBase { get; private set; }
    public string BaseCurrency { get; private set; } = "EUR";
    public decimal? FxRateSnapshot { get; private set; }
    public DateTime OrderedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    private Order() { }

    public static Order Create(
        Guid userId,
        Guid addressId,
        string currencyCode,
        IEnumerable<OrderItemLine> lines)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AddressId = addressId,
            CurrencyCode = currencyCode,
            Status = "pending",
            OrderedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        foreach (var line in lines)
        {
            var item = OrderItem.Create(
                order.Id,
                line.VariantId,
<<<<<<< HEAD
<<<<<<< HEAD
                line.ProductId,
                line.SellerId,
=======
>>>>>>> feature/orders-logic
=======
>>>>>>> feature/notifications-logic
                line.ProductName,
                line.ProductSku,
                line.Quantity,
                line.UnitPrice);
            order._items.Add(item);
        }

        order.TotalAmount = order._items.Sum(i => i.Quantity * i.UnitPrice);
        return order;
    }

    public void Confirm()
    {
        if (Status != "pending")
            throw new InvalidOperationException($"Cannot confirm order in status '{Status}'.");
        Status = "confirmed";
        UpdatedAt = DateTime.UtcNow;
    }

    public void Ship()
    {
        if (Status != "confirmed")
            throw new InvalidOperationException($"Cannot ship order in status '{Status}'.");
        Status = "shipped";
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status is "delivered" or "cancelled")
            throw new InvalidOperationException($"Cannot cancel order in status '{Status}'.");
        Status = "cancelled";
        UpdatedAt = DateTime.UtcNow;
    }
<<<<<<< HEAD
<<<<<<< HEAD

    public void SetPendingPayment()
    {
        Status = "pending_payment";
        UpdatedAt = DateTime.UtcNow;
    }

    public void Prepare()
    {
        if (Status != "confirmed")
            throw new InvalidOperationException($"Cannot prepare order in status '{Status}'.");
        Status = "preparing";
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkPaymentFailed()
    {
        if (Status is "delivered" or "cancelled")
            throw new InvalidOperationException($"Cannot mark payment failed for order in status '{Status}'.");
        Status = "payment_failed";
        UpdatedAt = DateTime.UtcNow;
    }
=======
>>>>>>> feature/orders-logic
=======
>>>>>>> feature/notifications-logic
}

public record OrderItemLine(
    Guid VariantId,
<<<<<<< HEAD
<<<<<<< HEAD
    Guid ProductId,
    Guid SellerId,
=======
>>>>>>> feature/orders-logic
=======
>>>>>>> feature/notifications-logic
    string ProductName,
    string ProductSku,
    int Quantity,
    decimal UnitPrice);
