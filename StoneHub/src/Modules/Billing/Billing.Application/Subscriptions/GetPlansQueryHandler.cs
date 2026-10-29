using Billing.Application.Dtos;
using Billing.Domain;
using MediatR;

namespace Billing.Application.Subscriptions;

// The paid plans, cheapest first, priced for this caller (launch offer only before their first
// payment, matching what StartCheckout attaches).
public sealed class GetPlansQueryHandler(IBillingQueryService billing, IPlanPricing pricing)
    : IRequestHandler<GetPlansQuery, IReadOnlyList<PlanOfferDto>>
{
    private static readonly SubscriptionPlan[] PaidPlans = [SubscriptionPlan.Basic, SubscriptionPlan.Premium];

    public async Task<IReadOnlyList<PlanOfferDto>> Handle(GetPlansQuery request, CancellationToken ct)
    {
        var subscription = await billing.GetByUserIdAsync(request.UserId, ct);
        var firstTime = subscription is null || !subscription.HasEverPaid;

        return PaidPlans
            .Select(plan => new PlanOfferDto(
                plan.ToString(),
                pricing.MonthlyPricePaise(plan),
                firstTime ? pricing.LaunchPricePaise(plan) : null))
            .ToList();
    }
}
