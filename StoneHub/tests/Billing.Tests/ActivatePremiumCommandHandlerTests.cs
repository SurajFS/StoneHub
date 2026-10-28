using Billing.Application.Subscriptions;
using Billing.Domain;
using Billing.Tests.Fakes;
using Xunit;

namespace Billing.Tests;

// Admin activation records an offline payment. If the seller is also auto-renewing through the
// gateway, that must stop first, or they're charged twice for the same Premium.
public sealed class ActivatePremiumCommandHandlerTests
{
    private const string GatewayId = "sub_first";

    private static readonly DateTimeOffset AdminExpiry = DateTimeOffset.UtcNow.AddDays(365);

    private readonly InMemoryBillingStore _store = new();
    private readonly FakePaymentGateway _gateway = new();

    [Fact]
    public async Task Handle_WhenGatewayRenewing_CancelsAtGatewayNow_ThenActivatesWithoutRenewal()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        subscription.ApplyGatewayCharge(GatewayId, DateTimeOffset.UtcNow.AddDays(30));
        _store.Add(subscription);

        var result = await HandleAsync(subscription.UserId);

        Assert.True(result.IsSuccess);
        Assert.Equal([GatewayId], _gateway.CancelledImmediately);
        Assert.True(subscription.IsPremiumActive);
        Assert.False(subscription.WillRenew);
        Assert.Equal(AdminExpiry, subscription.ExpiryDate);
    }

    [Fact]
    public async Task Handle_WhenRenewalAlreadyStopped_DoesNotCallGateway()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        subscription.ApplyGatewayCharge(GatewayId, DateTimeOffset.UtcNow.AddDays(30));
        subscription.StopRenewal();
        _store.Add(subscription);

        await HandleAsync(subscription.UserId);

        Assert.Empty(_gateway.CancelledImmediately);
        Assert.Equal(AdminExpiry, subscription.ExpiryDate);
    }

    [Fact]
    public async Task Handle_WhenFree_ActivatesWithoutCallingGateway()
    {
        var subscription = Subscription.CreateFree(Guid.NewGuid());
        _store.Add(subscription);

        var result = await HandleAsync(subscription.UserId);

        Assert.True(result.IsSuccess);
        Assert.Empty(_gateway.CancelledImmediately);
        Assert.True(subscription.IsPremiumActive);
    }

    [Fact]
    public async Task Handle_UnknownUser_ReturnsNotFound()
    {
        var result = await HandleAsync(Guid.NewGuid());

        Assert.Equal(SharedKernel.ErrorType.NotFound, result.ErrorType);
        Assert.Empty(_gateway.CancelledImmediately);
    }

    private Task<SharedKernel.Result> HandleAsync(Guid userId) =>
        new ActivatePremiumCommandHandler(_gateway, _store)
            .Handle(new ActivatePremiumCommand(userId, AdminExpiry, "annual, paid offline"), CancellationToken.None);
}
