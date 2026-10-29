using Billing.Domain;
using Identity.Application;

namespace Billing.Application;

public sealed class PlanEntitlements(IBillingQueryService billing, IUserDirectory users) : IPlanEntitlements
{
    private static readonly string[] OwnerRoles = ["Seller", "Wholesaler"];

    public async Task<bool> CanUseOwnerFeaturesAsync(Guid userId, CancellationToken ct = default)
    {
        var subscription = await billing.GetByUserIdAsync(userId, ct);
        if (subscription is not null)
            return subscription.Plan != nameof(SubscriptionPlan.Free)
                && subscription.Status == nameof(SubscriptionStatus.Active);

        // No row: buyers and admins never get one. An owner registered before Billing existed
        // doesn't either, and is treated as Free (checkout creates their row).
        var user = await users.GetAsync(userId, ct);
        return user is null || !OwnerRoles.Contains(user.Role, StringComparer.OrdinalIgnoreCase);
    }
}
