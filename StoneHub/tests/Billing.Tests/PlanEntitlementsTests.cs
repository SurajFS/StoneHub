using Billing.Application;
using Billing.Tests.Fakes;
using Xunit;

namespace Billing.Tests;

// The single rule every gate (listings, chat, inquiries) relies on: who may use the owner
// features. A wrong answer either gives paid features away or locks out paying sellers.
public sealed class PlanEntitlementsTests
{
    private readonly FakeBillingDirectory _directory = new();

    [Theory]
    [InlineData("Seller", "Basic", "Active", true)]
    [InlineData("Seller", "Premium", "Active", true)]
    [InlineData("Wholesaler", "Basic", "Active", true)]
    [InlineData("Seller", "Free", "Active", false)]
    [InlineData("Wholesaler", "Free", "Active", false)]
    [InlineData("Seller", "Free", "Expired", false)]
    [InlineData("Seller", "Free", "Cancelled", false)]
    public async Task OwnerWithSubscription_FollowsPlan(string role, string plan, string status, bool expected)
    {
        var userId = _directory.AddUser(role, plan, status);

        Assert.Equal(expected, await Entitlements().CanUseOwnerFeaturesAsync(userId));
    }

    [Theory]
    [InlineData("Buyer", true)]
    [InlineData("Admin", true)]
    [InlineData("Seller", false)]
    [InlineData("Wholesaler", false)]
    public async Task UserWithoutSubscriptionRow_BuyersAndAdminsAllowed_OwnersTreatedAsFree(string role, bool expected)
    {
        var userId = _directory.AddUser(role);

        Assert.Equal(expected, await Entitlements().CanUseOwnerFeaturesAsync(userId));
    }

    [Fact]
    public async Task UnknownUser_IsNotGated()
    {
        Assert.True(await Entitlements().CanUseOwnerFeaturesAsync(Guid.NewGuid()));
    }

    private PlanEntitlements Entitlements() => new(_directory, _directory);
}
