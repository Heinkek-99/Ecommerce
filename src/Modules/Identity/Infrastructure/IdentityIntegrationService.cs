using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Identity.Infrastructure;

public class IdentityIntegrationService : IIdentityIntegrationService
{
    private readonly IdentityDbContext _db;

    public IdentityIntegrationService(IdentityDbContext db) => _db = db;

    public async Task<Result> AddRoleAsync(Guid userId, string role, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null)
            return Result.Failure($"User {userId} not found.");

        user.AddRole(role);
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
