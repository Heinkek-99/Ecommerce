using Ecommerce.Cart.Application.Queries.GetCart;
using Xunit;
using Ecommerce.Cart.Application.Services;
using Ecommerce.Cart.Domain;
using FluentAssertions;
using NSubstitute;

namespace Ecommerce.Cart.Tests.Application;

public class GetCartHandlerTests
{
    private readonly ICartStorage _storage = Substitute.For<ICartStorage>();

    private GetCartHandler CreateHandler() => new(_storage);

    [Fact]
    public async Task Should_return_empty_cart_when_no_items()
    {
        var userId = Guid.NewGuid();
        _storage.GetAsync(userId, default).ReturnsForAnyArgs((ShoppingCart?)null);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetCartQuery(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.SubTotal.Should().Be(0m);
        result.Value.ItemCount.Should().Be(0);
    }

    [Fact]
    public async Task Should_return_cart_with_correct_subtotal()
    {
        var userId = Guid.NewGuid();
        var cart = new ShoppingCart(userId);
        var variantId = Guid.NewGuid();

        cart.AddItem(new CartItem(
            variantId, Guid.NewGuid(), Guid.NewGuid(),
            "Widget", "WGT-001", 25m, 3, null));

        _storage.GetAsync(userId, default).ReturnsForAnyArgs(cart);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetCartQuery(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.SubTotal.Should().Be(75m);   // 25 × 3
        result.Value.ItemCount.Should().Be(3);
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items[0].LineTotal.Should().Be(75m);
    }
}
