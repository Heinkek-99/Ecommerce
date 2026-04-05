namespace Ecommerce.Shared.Common;

public record ProductVariantInfo(
    Guid VariantId,
    Guid ProductId,
    Guid SellerId,
    string ProductName,
    string Sku,
    decimal Price,
    int StockQuantity,
    string? ImageUrl);
