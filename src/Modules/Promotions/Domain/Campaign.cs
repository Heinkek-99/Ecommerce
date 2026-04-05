namespace Ecommerce.Promotions.Domain;

public class Campaign
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string Type { get; private set; } = "seasonal";
    public DateTime StartsAt { get; private set; }
    public DateTime EndsAt { get; private set; }
    public bool IsActive { get; private set; }
    public int Priority { get; private set; }

    private Campaign() { }

    public static Campaign Create(
        string name,
        string type,
        DateTime startsAt,
        DateTime endsAt,
        int priority = 0)
    {
        return new Campaign
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = type,
            StartsAt = startsAt,
            EndsAt = endsAt,
            IsActive = true,
            Priority = priority
        };
    }
}
