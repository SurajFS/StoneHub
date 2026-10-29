using Billing.Domain;
using MediatR;
using SharedKernel;

namespace Billing.Application.Subscriptions;

// Admin-triggered paid plan for payments taken outside the gateway (bank transfer, cash, ...) or
// a granted period. Replaces any gateway auto-renewal the owner had.
public sealed record ActivatePlanCommand(Guid UserId, SubscriptionPlan Plan, DateTimeOffset ExpiryDate, string? PaymentNotes)
    : IRequest<Result>;
