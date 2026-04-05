using System.Text.Json.Serialization;

namespace Ecommerce.Cart.Domain;

public class ShoppingCart
{
    [JsonInclude]
    public Guid UserId { get; private set; }

    [JsonInclude]
    public List<CartItem> Items { get; private set; } = new();

    [JsonInclude]
    public DateTime UpdatedAt { get; private set; }

    public ShoppingCart(Guid userId)
    {
        UserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    // For System.Text.Json deserialization
    [JsonConstructor]
    private ShoppingCart(Guid userId, List<CartItem> items, DateTime updatedAt)
    {
        UserId = userId;
        Items = items;
        UpdatedAt = updatedAt;
    }

    public void AddItem(CartItem item)
    {
        var existing = Items.FirstOrDefault(i => i.VariantId == item.VariantId);
        if (existing is not null)
        {
            var idx = Items.IndexOf(existing);
            Items[idx] = existing with { Quantity = existing.Quantity + item.Quantity };
        }
        else
        {
            Items.Add(item);
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateQuantity(Guid variantId, int newQuantity)
    {
        var existing = Items.FirstOrDefault(i => i.VariantId == variantId);
        if (existing is null) return;

        if (newQuantity <= 0)
            Items.Remove(existing);
        else
        {
            var idx = Items.IndexOf(existing);
            Items[idx] = existing with { Quantity = newQuantity };
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveItem(Guid variantId)
    {
        Items.RemoveAll(i => i.VariantId == variantId);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Clear()
    {
        Items.Clear();
        UpdatedAt = DateTime.UtcNow;
    }

    public decimal GetTotal() => Items.Sum(i => i.UnitPrice * i.Quantity);
}
