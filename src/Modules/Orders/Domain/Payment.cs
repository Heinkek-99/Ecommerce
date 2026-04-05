namespace Ecommerce.Orders.Domain;

public class Payment
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public string Method { get; private set; } = default!;
    public string Status { get; private set; } = "pending";
    public decimal Amount { get; private set; }
    public string? TransactionRef { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public string? CurrencyCode { get; private set; }
    public decimal? AmountInBase { get; private set; }
    public decimal? FxRate { get; private set; }

    private Payment() { }

    public static Payment Create(Guid orderId, string method, decimal amount, string? currencyCode = null)
    {
        return new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Method = method,
            Amount = amount,
            Status = "pending",
            CurrencyCode = currencyCode
        };
    }

    public void Complete(string transactionRef)
    {
        Status = "completed";
        TransactionRef = transactionRef;
        PaidAt = DateTime.UtcNow;
    }

    public void Fail() => Status = "failed";
    public void Refund() => Status = "refunded";
}
