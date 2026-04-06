namespace Ecommerce.Catalog.Domain;

public class Product
{
    public Guid Id { get; private set; }
    public Guid SellerId { get; private set; }
    public string SellerName { get; private set; } = default!;   // snapshot
    public Guid? CategoryId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public string? Description { get; private set; }
    public decimal BasePrice { get; private set; }
    public string? MainImageUrl { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<ProductVariant> _variants = new();
    public IReadOnlyList<ProductVariant> Variants => _variants.AsReadOnly();

    private Product() { }

    public static Product Create(
        Guid sellerId,
        string sellerName,
        Guid? categoryId,
        string name,
        string slug,
        string? description,
        decimal basePrice,
        string? mainImageUrl = null)
    {
        if (basePrice < 0)
            throw new ArgumentException("Base price cannot be negative.", nameof(basePrice));

        return new Product
        {
            Id = Guid.NewGuid(),
            SellerId = sellerId,
            SellerName = sellerName,
            CategoryId = categoryId,
            Name = name,
            Slug = slug,
            Description = description,
            BasePrice = basePrice,
            MainImageUrl = mainImageUrl,
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
