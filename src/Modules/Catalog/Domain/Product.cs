namespace Ecommerce.Catalog.Domain;

public class Product
{
    public Guid Id { get; private set; }
    public Guid SellerId { get; private set; }
    public Guid? CategoryId { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public decimal BasePrice { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<ProductVariant> _variants = new();
    public IReadOnlyList<ProductVariant> Variants => _variants.AsReadOnly();

    private Product() { }

    public static Product Create(
        Guid sellerId,
        Guid? categoryId,
        string name,
        string? description,
        decimal basePrice)
    {
        if (basePrice < 0)
            throw new ArgumentException("Base price cannot be negative.", nameof(basePrice));

        return new Product
        {
            Id = Guid.NewGuid(),
            SellerId = sellerId,
            CategoryId = categoryId,
            Name = name,
            Description = description,
            BasePrice = basePrice,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(newPrice));
        BasePrice = newPrice;
        UpdatedAt = DateTime.UtcNow;
    }
}
