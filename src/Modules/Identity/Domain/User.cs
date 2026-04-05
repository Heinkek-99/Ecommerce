namespace Ecommerce.Identity.Domain;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = default!;
    public string FullName { get; private set; } = default!;
    public string? Phone { get; private set; }
    public string Role { get; private set; } = "buyer";
    public string PasswordHash { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private User() { }

<<<<<<< HEAD
    /// <summary>
    /// Ajoute un rôle à cet utilisateur (multi-rôle comma-separated).
    /// Idempotent : ne duplique pas si le rôle existe déjà.
    /// </summary>
    public void AddRole(string role)
    {
        var roles = Role
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet();
        roles.Add(role.Trim());
        Role = string.Join(",", roles);
        UpdatedAt = DateTime.UtcNow;
    }

    public IEnumerable<string> GetRoles() =>
        Role.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

=======
>>>>>>> feature/orders-logic
    public static User Create(
        string email,
        string fullName,
        string passwordHash,
        string? phone = null,
        string role = "buyer")
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            FullName = fullName,
            PasswordHash = passwordHash,
            Phone = phone,
            Role = role,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }
}
