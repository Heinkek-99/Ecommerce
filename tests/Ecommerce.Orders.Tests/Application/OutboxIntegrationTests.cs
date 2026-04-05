using Ecommerce.Orders.Domain;
using Ecommerce.Shared.Events;
using FluentAssertions;

namespace Ecommerce.Orders.Tests.Application;

/// <summary>
/// Unit-level outbox tests verifying the event creation logic.
/// Full outbox integration tests (verifying wolverine.outbox_messages in PostgreSQL)
/// are in Ecommerce.Api.IntegrationTests.
/// </summary>
public class OutboxIntegrationTests
{
    [Fact]
    public void OrderConfirmedEvent_HasCorrectData()
    {
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        const decimal total = 149.90m;

        var evt = new OrderConfirmedEvent(
            orderId, userId, total, "EUR", "SUMMER20", DateTime.UtcNow);

        evt.OrderId.Should().Be(orderId);
        evt.UserId.Should().Be(userId);
        evt.TotalAmount.Should().Be(total);
        evt.CurrencyCode.Should().Be("EUR");
        evt.PromoCode.Should().Be("SUMMER20");
        evt.Id.Should().NotBeEmpty();
        evt.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void PlaceOrder_Creates_Order_With_CorrectItems_For_Outbox()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var addressId = Guid.NewGuid();
        var lines = new List<OrderItemLine>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Laptop Pro", "LAP-001", 1, 999.99m),
            new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Mouse", "MSE-002", 2, 29.99m)
        };

        // Act
        var order = Order.Create(userId, addressId, "EUR", lines);

        // Assert — the event would contain these values
        order.Id.Should().NotBeEmpty();
        order.TotalAmount.Should().Be(1059.97m);
        order.Items.Should().HaveCount(2);
        order.Items[0].ProductName.Should().Be("Laptop Pro");
        order.Items[1].ProductSku.Should().Be("MSE-002");
    }
}
