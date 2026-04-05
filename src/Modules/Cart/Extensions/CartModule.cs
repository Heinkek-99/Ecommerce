using Ecommerce.Cart.Application.Services;
using Ecommerce.Cart.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Cart.Extensions;

public static class CartModule
{
    public static IServiceCollection AddCartModule(this IServiceCollection services)
    {
        services.AddScoped<ICartStorage, RedisCartStorage>();
        return services;
    }
}
