using Ecommerce.Cart.Application.Commands.AddToCart;
using Xunit;
using Ecommerce.Cart.Application.Services;
using Ecommerce.Cart.Domain;
using Ecommerce.Shared.Abstractions;
using Ecommerce.Shared.Common;
using FluentAssertions;
using NSubstitute;

namespace Ecommerce.Cart.Tests.Application;

public class AddToCartHandlerTests
{
    private readonly ICartStorage _storage = Substitute.For<ICartStorage>();
    private readonly ICatalogIntegrationService _catalog = Substitute.For<ICatalogIntegrationService>();

    private AddToCartHandler CreateHandler() => new(_storage, _catalog);

    private static ProductVariantInfo MakeVariantInfo(int stock = 10) =>
        new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Widget Pro", "WGT-001", 49.99m, stock, null);

    [Fact]
    public async Task Should_add_item_when_stock_available()
    {
        var userId = Guid.NewGuid();
        var variantInfo = MakeVariantInfo(stock: 5);
        var cmd = new AddToCartCommand(userId, variantInfo.VariantId, 2);

        _catalog.GetVariantInfoAsync(variantInfo.VariantId, default)
            .ReturnsForAnyArgs(variantInfo);
        _storage.GetAsync(userId, default).ReturnsForAnyArgs((ShoppingCart?)null);

        var handler = CreateHandler();
        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _storage.ReceivedWithAnyArgs(1).SetAsync(Arg.Any<ShoppingCart>(), default);
    }

    [Fact]
    public async Task Should_fail_when_stock_insufficient()
    {
        var variantInfo = MakeVariantInfo(stock: 1);
        var cmd = new AddToCartCommand(Guid.NewGuid(), variantInfo.VariantId, 5);

        _catalog.GetVariantInfoAsync(variantInfo.VariantId, default)
            .ReturnsForAnyArgs(variantInfo);

        var handler = CreateHandler();
        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Insufficient stock");
    }

    [Fact]
    public async Task Should_increment_quantity_when_item_already_in_cart()
    {
        var userId = Guid.NewGuid();
        var variantInfo = MakeVariantInfo(stock: 20);
        var cmd = new AddToCartCommand(userId, variantInfo.VariantId, 3);

        var existingCart = new ShoppingCart(userId);
        existingCart.AddItem(new CartItem(
            variantInfo.VariantId, variantInfo.ProductId, variantInfo.SellerId,
            variantInfo.ProductName, variantInfo.Sku,
            variantInfo.Price, 2, null));

        _catalog.GetVariantInfoAsync(variantInfo.VariantId, default)
            .ReturnsForAnyArgs(variantInfo);
        _storage.GetAsync(userId, default).ReturnsForAnyArgs(existingCart);

        ShoppingCart? savedCart = null;
        await _storage.SetAsync(
            Arg.Do<ShoppingCart>(c => savedCart = c),
            Arg.Any<CancellationToken>());

        var handler = CreateHandler();
        await handler.Handle(cmd, CancellationToken.None);

        savedCart.Should().NotBeNull();
        savedCart!.Items.Should().HaveCount(1);
        savedCart.Items[0].Quantity.Should().Be(5); // 2 + 3
    }
}
