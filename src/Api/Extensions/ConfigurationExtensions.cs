namespace Ecommerce.Api.Extensions;

public static class ConfigurationExtensions
{
    /// <summary>
    /// Maps .env variable names to the nested configuration keys used throughout the app.
    /// Called after DotNetEnv.Env.Load() so environment variables are already set.
    /// </summary>
    public static void MapEnvToConfiguration(this IConfigurationBuilder config)
    {
        var mappings = new Dictionary<string, string?>
        {
            ["ConnectionStrings:Postgres"]  = Env("POSTGRES_CONNECTION"),
            ["ConnectionStrings:Redis"]     = Env("REDIS_CONNECTION"),
            ["JwtSettings:SecretKey"]       = Env("JWT_SECRET"),
            ["JwtSettings:Issuer"]          = Env("JWT_ISSUER"),
            ["JwtSettings:Audience"]        = Env("JWT_AUDIENCE"),
            ["Stripe:SecretKey"]            = Env("STRIPE_SECRET_KEY"),
            ["Stripe:PublishableKey"]       = Env("STRIPE_PUBLISHABLE_KEY"),
            ["Stripe:WebhookSecret"]        = Env("STRIPE_WEBHOOK_SECRET"),
            ["Stripe:CommissionRate"]       = Env("STRIPE_COMMISSION_RATE"),
        };

        var nonNull = mappings
            .Where(kv => kv.Value is not null)
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        config.AddInMemoryCollection(nonNull);
    }

    private static string? Env(string key) => Environment.GetEnvironmentVariable(key);
}
