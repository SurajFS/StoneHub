using Billing.Application.Dtos;
using MediatR;
using SharedKernel;

namespace Billing.Application.Subscriptions;

public sealed record GetMySubscriptionQuery(Guid UserId) : IRequest<Result<SubscriptionDto>>;
