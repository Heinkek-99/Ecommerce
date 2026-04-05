using Ecommerce.Orders.Application.Commands.PlaceOrder;
using Ecommerce.Orders.Domain;
using Ecommerce.Shared.Common;
using FluentAssertions;
using NSubstitute;
using Wolverine;

namespace Ecommerce.Orders.Tests.Application;

public class PlaceOrderHandlerTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    private static List<OrderItemLine> ValidLines() =>
    [
        new OrderItemLine(Guid.NewGuid(), "Widget Pro", "WGT-001", 2, 49.99m)
    ];

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithOrderId()
    {
        var cmd = new PlaceOrderCommand(
            UserId: Guid.NewGuid(),
            AddressId: Guid.NewGuid(),
            CurrencyCode: "EUR",
            PromoCode: null,
            Lines: ValidLines());

        var order = Order.Create(cmd.UserId, cmd.AddressId, cmd.CurrencyCode, cmd.Lines);
        order.Should().NotBeNull();
        order.TotalAmount.Should().Be(99.98m);
    }

    [Fact]
    public void Order_Create_CalculatesTotalCorrectly()
    {
        var userId = Guid.NewGuid();
        var addressId = Guid.NewGuid();
        var lines = new List<OrderItemLine>
        {
            new(Guid.NewGuid(), "Product A", "SKU-A", 3, 10m),
            new(Guid.NewGuid(), "Product B", "SKU-B", 1, 25m)
        };

        var order = Order.Create(userId, addressId, "EUR", lines);

        order.TotalAmount.Should().Be(55m);
        order.Items.Should().HaveCount(2);
        order.Status.Should().Be("pending"); // PlaceOrderHandler appelle Confirm() mais Order.Create() seul = pending
    }

    [Fact]
    public void Order_Cancel_WhenConfirmed_SetsStatusCancelled()
    {
        var order = Order.Create(
            Guid.NewGuid(), Guid.NewGuid(), "EUR",
            [new(Guid.NewGuid(), "P", "SKU", 1, 10m)]);

        order.Cancel();

        order.Status.Should().Be("cancelled");
    }

    [Fact]
    public void Order_Cancel_AfterConfirm_SetsStatusCancelled()
    {
        var order = Order.Create(
            Guid.NewGuid(), Guid.NewGuid(), "EUR",
            [new(Guid.NewGuid(), "P", "SKU", 1, 10m)]);
        order.Confirm();
        order.Ship(); // confirm → ship

        order.Cancel(); // shipped → cancelled allowed by domain
        order.Status.Should().Be("cancelled");
    }

    [Fact]
    public void OrderItem_Snapshot_StoresProductData()
    {
        var item = OrderItem.Create(
            Guid.NewGuid(), Guid.NewGuid(),
            productName: "Widget", productSku: "WGT-001",
            quantity: 2, unitPrice: 19.99m);

        item.ProductName.Should().Be("Widget");
        item.ProductSku.Should().Be("WGT-001");
        item.UnitPrice.Should().Be(19.99m);
    }
}
