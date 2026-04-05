using System.Text.Json;
using Ecommerce.Cart.Application.Services;
using Ecommerce.Cart.Domain;
using Microsoft.Extensions.Caching.Distributed;

namespace Ecommerce.Cart.Infrastructure;

public class RedisCartStorage : ICartStorage
{
    private readonly IDistributedCache _cache;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly DistributedCacheEntryOptions _cacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
    };

    public RedisCartStorage(IDistributedCache cache) => _cache = cache;

    private static string Key(Guid userId) => $"cart:{userId}";

    public async Task<ShoppingCart?> GetAsync(Guid userId, CancellationToken ct)
    {
        var data = await _cache.GetStringAsync(Key(userId), ct);
        if (data is null) return null;
        return JsonSerializer.Deserialize<ShoppingCart>(data, _jsonOptions);
    }

    public async Task SetAsync(ShoppingCart cart, CancellationToken ct)
    {
        var data = JsonSerializer.Serialize(cart, _jsonOptions);
        await _cache.SetStringAsync(Key(cart.UserId), data, _cacheOptions, ct);
    }

    public async Task DeleteAsync(Guid userId, CancellationToken ct)
    {
        await _cache.RemoveAsync(Key(userId), ct);
    }

    public async Task<bool> ExistsAsync(Guid userId, CancellationToken ct)
    {
        var data = await _cache.GetAsync(Key(userId), ct);
        return data is not null;
    }
}
