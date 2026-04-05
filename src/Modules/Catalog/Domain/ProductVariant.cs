namespace Ecommerce.Catalog.Domain;

public class ProductVariant
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public string Sku { get; private set; } = default!;
    public string? Color { get; private set; }
    public string? Size { get; private set; }
    public decimal? Price { get; private set; }
    public int StockQuantity { get; private set; }

    private ProductVariant() { }

    public static ProductVariant Create(
        Guid productId,
        string sku,
        int stockQuantity,
        decimal? price = null,
        string? color = null,
        string? size = null)
    {
        return new ProductVariant
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Sku = sku,
            StockQuantity = stockQuantity,
            Price = price,
            Color = color,
            Size = size
        };
    }

    public void AdjustStock(int delta)
    {
        if (StockQuantity + delta < 0)
            throw new InvalidOperationException("Insufficient stock.");
        StockQuantity += delta;
    }
}
