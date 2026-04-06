using Ecommerce.Loyalty.Domain;
using FluentAssertions;

namespace Ecommerce.Loyalty.Tests.Application;

/// <summary>
/// Vérifie que le traitement double du même OrderConfirmedEvent est ignoré.
/// Pattern idempotence manuelle en complément du Wolverine Inbox.
/// </summary>
public class IdempotencyTests
{
    [Fact]
    public void OrderId_AlreadyProcessed_ShouldNotAddDuplicateTransaction()
    {
        // Simule la logique de vérification d'idempotence du handler
        var orderId = Guid.NewGuid();
        var account = LoyaltyAccount.Create(Guid.NewGuid(), Guid.NewGuid());

        // Premier traitement
        var alreadyProcessed = account.Transactions.Any(t => t.OrderId == orderId && t.Type == "earn");
        if (!alreadyProcessed)
            account.AddPoints(100, orderId);

        // Deuxième traitement (doublon)
        alreadyProcessed = account.Transactions.Any(t => t.OrderId == orderId && t.Type == "earn");
        if (!alreadyProcessed)
            account.AddPoints(100, orderId);

        // Ne doit créer qu'une seule transaction
        account.Transactions.Should().HaveCount(1);
        account.PointsBalance.Should().Be(100);
        account.PointsLifetime.Should().Be(100);
    }

    [Fact]
    public void SameOrderId_ProcessedTwice_BalanceUnchangedSecondTime()
    {
        var orderId = Guid.NewGuid();
        var account = LoyaltyAccount.Create(Guid.NewGuid(), Guid.NewGuid());

        void ProcessEvent(Guid oid, int points)
        {
            var exists = account.Transactions.Any(t => t.OrderId == oid && t.Type == "earn");
            if (!exists)
                account.AddPoints(points, oid);
        }

        // Simule deux appels au handler avec le même event
        ProcessEvent(orderId, 500);
        ProcessEvent(orderId, 500);

        account.Transactions.Should().HaveCount(1);
        account.PointsBalance.Should().Be(500);
    }

    [Fact]
    public void DifferentOrderIds_BothProcessed_TwoTransactionsCreated()
    {
        var orderId1 = Guid.NewGuid();
        var orderId2 = Guid.NewGuid();
        var account = LoyaltyAccount.Create(Guid.NewGuid(), Guid.NewGuid());

        account.AddPoints(100, orderId1);
        account.AddPoints(200, orderId2);

        account.Transactions.Should().HaveCount(2);
        account.PointsBalance.Should().Be(300);
    }
}
