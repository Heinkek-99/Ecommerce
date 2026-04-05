using Ecommerce.Identity.Domain;
using Ecommerce.Identity.Infrastructure;
using Ecommerce.Shared.Common;
using Ecommerce.Shared.Events;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Ecommerce.Identity.Application.Commands.RegisterUser;

public class RegisterUserHandler
{
    private readonly IdentityDbContext _db;

    public RegisterUserHandler(IdentityDbContext db) => _db = db;

    public async Task<Result<Guid>> Handle(
        RegisterUserCommand cmd,
        IMessageBus bus,
        CancellationToken ct)
    {
        var emailGuard = Guard.NotEmpty(cmd.Email, nameof(cmd.Email));
        if (emailGuard.IsFailure) return Result.Failure<Guid>(emailGuard.Error!);

        var passwordGuard = Guard.NotEmpty(cmd.Password, nameof(cmd.Password));
        if (passwordGuard.IsFailure) return Result.Failure<Guid>(passwordGuard.Error!);

        var exists = await _db.Users.AnyAsync(u => u.Email == cmd.Email.ToLowerInvariant(), ct);
        if (exists) return Result.Failure<Guid>("Email already registered.");

        var hasher = new PasswordHasher<User>();
        var passwordHash = hasher.HashPassword(null!, cmd.Password);

        var user = User.Create(cmd.Email, cmd.FullName, passwordHash, cmd.Phone, cmd.Role);
        _db.Users.Add(user);

        // OUTBOX durable : IdentityDbContext est enrôlé via AddDbContextWithWolverineIntegration
        // → le message est écrit dans wolverine_outgoing_envelopes dans la MÊME transaction
        await bus.PublishAsync(new UserRegisteredEvent(
            user.Id, user.Email, user.FullName, DateTime.UtcNow));

        await _db.SaveChangesAsync(ct);

        return Result.Success(user.Id);
    }
}
