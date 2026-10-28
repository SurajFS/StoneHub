using MediatR;
using SharedKernel;

namespace Billing.Application.Subscriptions;

// Admin-triggered Premium for payments taken outside the gateway (bank transfer, cash, ...).
// Replaces any gateway auto-renewal the owner had.
public sealed record ActivatePremiumCommand(Guid UserId, DateTimeOffset ExpiryDate, string? PaymentNotes)
    : IRequest<Result>;
