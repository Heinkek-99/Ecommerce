namespace Ecommerce.Catalog.Domain;

public class Seller
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string BusinessName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string? Phone { get; private set; }
    public string? Address { get; private set; }
    public string? StripeAccountId { get; private set; }
    public bool IsVerified { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Seller() { }

    public static Seller Create(Guid userId, string businessName, string email,
        string? phone = null, string? address = null)
    {
        return new Seller
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BusinessName = businessName,
            Email = email,
            Phone = phone,
            Address = address,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void ConnectStripe(string accountId)
    {
        StripeAccountId = accountId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsVerified()
    {
        IsVerified = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateVerification(bool isVerified)
    {
        IsVerified = isVerified;
        UpdatedAt = DateTime.UtcNow;
    }
}
