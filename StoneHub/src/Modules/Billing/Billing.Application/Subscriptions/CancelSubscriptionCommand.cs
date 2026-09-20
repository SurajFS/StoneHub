using MediatR;
using SharedKernel;

namespace Billing.Application.Subscriptions;

public sealed record CancelSubscriptionCommand(Guid UserId) : IRequest<Result>;
