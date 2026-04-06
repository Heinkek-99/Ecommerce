namespace Ecommerce.Orders.Domain;

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid VariantId { get; private set; }

    // Snapshot — données figées au moment de la commande
<<<<<<< HEAD
<<<<<<< HEAD
    public Guid ProductId { get; private set; }
    public Guid SellerId { get; private set; }
=======
>>>>>>> feature/orders-logic
=======
>>>>>>> feature/notifications-logic
    public string ProductName { get; private set; } = default!;
    public string ProductSku { get; private set; } = default!;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    private OrderItem() { }

    public static OrderItem Create(
        Guid orderId,
        Guid variantId,
<<<<<<< HEAD
<<<<<<< HEAD
        Guid productId,
        Guid sellerId,
=======
>>>>>>> feature/orders-logic
=======
>>>>>>> feature/notifications-logic
        string productName,
        string productSku,
        int quantity,
        decimal unitPrice)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be > 0.", nameof(quantity));
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));

        return new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            VariantId = variantId,
<<<<<<< HEAD
<<<<<<< HEAD
            ProductId = productId,
            SellerId = sellerId,
=======
>>>>>>> feature/orders-logic
=======
>>>>>>> feature/notifications-logic
            ProductName = productName,
            ProductSku = productSku,
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }
}
