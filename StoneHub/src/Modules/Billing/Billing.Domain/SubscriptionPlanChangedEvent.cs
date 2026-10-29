using SharedKernel;

namespace Billing.Domain;

// A paid account moved between paid plans (Basic ↔ Premium) without passing through Free, e.g.
// it resubscribed to the other plan after stopping renewal. Premium-only benefits (ranking,
// Sponsored campaigns) follow Plan.
public sealed record SubscriptionPlanChangedEvent(Guid UserId, SubscriptionPlan Plan) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
