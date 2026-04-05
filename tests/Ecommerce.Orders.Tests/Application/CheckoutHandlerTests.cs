using Ecommerce.Orders.Application.Commands.Checkout;
using Ecommerce.Orders.Infrastructure;
using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Ecommerce.Orders.Tests.Application;

public class CheckoutHandlerTests
{
    private static OrdersDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new OrdersDbContext(options);
    }

    private static List<CheckoutItemLine> ValidItems() =>
    [
        new CheckoutItemLine(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Widget Pro", "WGT-001", 49.99m, 2)
    ];

    [Fact]
    public async Task Should_create_order_with_pending_payment_status()
    {
        using var db = CreateInMemoryDb();
        var catalog = Substitute.For<ICatalogIntegrationService>();
        var stripe = Substitute.For<IStripePaymentService>();

        catalog.DecrementStockAsync(default!, default)
            .ReturnsForAnyArgs(Result.Success());
        stripe.CreatePaymentIntentAsync(default, default!, default!, default, default!, default)
            .ReturnsForAnyArgs(Result.Success(new PaymentIntentResult("pi_test", "secret_test", "requires_payment_method")));

        var handler = new CheckoutHandler(db, catalog, stripe);
        var cmd = new CheckoutCommand(
            Guid.NewGuid(), Guid.NewGuid(), "EUR", ValidItems(), null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var order = await db.Orders.FirstAsync();
        order.Status.Should().Be("pending_payment");
    }

    [Fact]
    public async Task Should_reserve_stock_on_checkout()
    {
        using var db = CreateInMemoryDb();
        var catalog = Substitute.For<ICatalogIntegrationService>();
        var stripe = Substitute.For<IStripePaymentService>();

        catalog.DecrementStockAsync(default!, default)
            .ReturnsForAnyArgs(Result.Success());
        stripe.CreatePaymentIntentAsync(default, default!, default!, default, default!, default)
            .ReturnsForAnyArgs(Result.Success(new PaymentIntentResult("pi_test", "secret_test", "requires_payment_method")));

        var handler = new CheckoutHandler(db, catalog, stripe);
        var cmd = new CheckoutCommand(
            Guid.NewGuid(), Guid.NewGuid(), "EUR", ValidItems(), null);

        await handler.Handle(cmd, CancellationToken.None);

        await catalog.ReceivedWithAnyArgs(1)
            .DecrementStockAsync(Arg.Any<IEnumerable<(Guid, int)>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_return_client_secret_from_stripe()
    {
        using var db = CreateInMemoryDb();
        var catalog = Substitute.For<ICatalogIntegrationService>();
        var stripe = Substitute.For<IStripePaymentService>();

        catalog.DecrementStockAsync(default!, default)
            .ReturnsForAnyArgs(Result.Success());
        stripe.CreatePaymentIntentAsync(default, default!, default!, default, default!, default)
            .ReturnsForAnyArgs(Result.Success(new PaymentIntentResult("pi_123", "sk_test_secret", "requires_payment_method")));

        var handler = new CheckoutHandler(db, catalog, stripe);
        var cmd = new CheckoutCommand(
            Guid.NewGuid(), Guid.NewGuid(), "EUR", ValidItems(), null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ClientSecret.Should().Be("sk_test_secret");
        result.Value.OrderId.Should().NotBeEmpty();
    }
}
