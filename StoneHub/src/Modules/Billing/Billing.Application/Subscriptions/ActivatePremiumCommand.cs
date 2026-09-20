using MediatR;
using SharedKernel;

namespace Billing.Application.Subscriptions;

// Admin-triggered stand-in for a real payment gateway checkout (see task.md — payments deferred).
public sealed record ActivatePremiumCommand(Guid UserId, DateTimeOffset ExpiryDate, string? PaymentNotes)
    : IRequest<Result>;
