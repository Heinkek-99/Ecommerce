namespace Ecommerce.Cart.Domain;

public record CartItem(
    Guid VariantId,
    Guid ProductId,
    Guid SellerId,
    string ProductName,
    string Sku,
    decimal UnitPrice,
    int Quantity,
    string? ImageUrl);
