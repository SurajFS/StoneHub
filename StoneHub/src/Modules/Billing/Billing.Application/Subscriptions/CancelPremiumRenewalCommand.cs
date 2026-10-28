using MediatR;
using SharedKernel;

namespace Billing.Application.Subscriptions;

// Owner-initiated: Premium runs to the end of the paid period, then isn't renewed.
public sealed record CancelPremiumRenewalCommand(Guid UserId) : IRequest<Result>;
