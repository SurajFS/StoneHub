using MediatR;
using SharedKernel;

namespace Billing.Application.Subscriptions;

// Owner-initiated: the paid plan runs to the end of the paid period, then isn't renewed.
public sealed record CancelRenewalCommand(Guid UserId) : IRequest<Result>;
