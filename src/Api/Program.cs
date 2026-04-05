using System.Text;
using DotNetEnv;
using Ecommerce.Api.Extensions;
using Ecommerce.Cart.Extensions;
using Ecommerce.Infrastructure.Stripe;
using Ecommerce.Shared.Abstractions;
using Stripe;
using Ecommerce.Catalog.Extensions;
using Ecommerce.Identity.Extensions;
using Ecommerce.Infrastructure.Cache;
using Ecommerce.Infrastructure.Wolverine;
using Ecommerce.Loyalty.Extensions;
using Ecommerce.Notifications.Extensions;
using Ecommerce.Orders.Extensions;
using Ecommerce.Promotions.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Wolverine;

// Load .env before anything else (no-op if file absent in prod)
Env.Load();

// 1. Serilog early init
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Map .env variables → IConfiguration nested keys
    builder.Configuration.AddEnvironmentVariables();
    builder.Configuration.MapEnvToConfiguration();

    // 1. Serilog
    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console());

    var connectionString = builder.Configuration.GetConnectionString("Postgres")
        ?? "Host=localhost;Port=5432;Database=ecommerce;Username=postgres;Password=postgres";

    var redisConnection = builder.Configuration.GetConnectionString("Redis")
        ?? "localhost:6379";

    // 2. DbContexts (modules)
    builder.Services.AddIdentityModule(connectionString);
    builder.Services.AddCatalogModule(connectionString);
    builder.Services.AddOrdersModule(connectionString);
    builder.Services.AddPromotionsModule(connectionString);
    builder.Services.AddLoyaltyModule(connectionString);
    builder.Services.AddNotificationsModule(connectionString);
    builder.Services.AddCartModule();

    // Stripe
    var stripeSettings = builder.Configuration.GetSection("Stripe").Get<StripeSettings>()
        ?? new StripeSettings(string.Empty, string.Empty, string.Empty, 0.05m);
    StripeConfiguration.ApiKey = stripeSettings.SecretKey;
    builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
    builder.Services.AddScoped<IStripePaymentService, StripePaymentService>();
    builder.Services.AddScoped<IStripeConnectService, StripeConnectService>();

    // 3. Redis
    builder.Services.AddStackExchangeRedisCache(opts => opts.Configuration = redisConnection);
    builder.Services.AddSingleton<RedisCacheService>();

    // 4. FluentValidation — scan tous les assemblies des modules
    builder.Services.AddValidatorsFromAssemblyContaining<Ecommerce.Catalog.Application.AssemblyMarker>();
    builder.Services.AddValidatorsFromAssemblyContaining<Ecommerce.Orders.Application.AssemblyMarker>();
    builder.Services.AddValidatorsFromAssemblyContaining<Ecommerce.Promotions.Application.AssemblyMarker>();
    builder.Services.AddValidatorsFromAssemblyContaining<Ecommerce.Loyalty.Application.AssemblyMarker>();
    builder.Services.AddValidatorsFromAssemblyContaining<Ecommerce.Notifications.Application.AssemblyMarker>();

    // 5. JWT Bearer
    var jwtKey = builder.Configuration["JwtSettings:SecretKey"]
        ?? "ecommerce-super-secret-key-32chars!!";
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(opts =>
        {
            opts.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey))
            };
        });
    builder.Services.AddAuthorization();

    // 6. Wolverine — Outbox/Inbox PostgreSQL + découverte handlers de tous les modules
    builder.Host.UseWolverine(opts =>
    {
        opts.AddWolverineConfiguration(connectionString);

        opts.Discovery.IncludeAssembly(
            typeof(Ecommerce.Identity.Application.AssemblyMarker).Assembly);
        opts.Discovery.IncludeAssembly(
            typeof(Ecommerce.Catalog.Application.AssemblyMarker).Assembly);
        opts.Discovery.IncludeAssembly(
            typeof(Ecommerce.Orders.Application.AssemblyMarker).Assembly);
        opts.Discovery.IncludeAssembly(
            typeof(Ecommerce.Promotions.Application.AssemblyMarker).Assembly);
        opts.Discovery.IncludeAssembly(
            typeof(Ecommerce.Loyalty.Application.AssemblyMarker).Assembly);
        opts.Discovery.IncludeAssembly(
            typeof(Ecommerce.Notifications.Application.AssemblyMarker).Assembly);
        opts.Discovery.IncludeAssembly(
            typeof(Ecommerce.Cart.Application.AssemblyMarker).Assembly);

        // Jobs récurrents : enregistrés comme IHostedService
        // (Wolverine Scheduler API for periodic jobs requires separate configuration per version)
    });

    // Jobs récurrents comme BackgroundService
    builder.Services.AddHostedService<Ecommerce.Api.BackgroundServices.NotificationsBackgroundService>();
    builder.Services.AddHostedService<Ecommerce.Api.BackgroundServices.LoyaltyExpiryBackgroundService>();

    // 7. Swagger + Controllers
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { Title = "Ecommerce API", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Description = "JWT token. Exemple: Bearer eyJ...",
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });
        c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    // Health checks
    builder.Services.AddHealthChecks();

    var app = builder.Build();

    // 8. Migrations au démarrage (SAUF wolverine — géré automatiquement)
    await app.ApplyMigrationsAsync();

    // 9. Seed data (Development uniquement — idempotent)
    if (app.Environment.IsDevelopment())
        await SeedDataExtensions.SeedDevelopmentDataAsync(app.Services);

    // 9. Middleware pipeline
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
