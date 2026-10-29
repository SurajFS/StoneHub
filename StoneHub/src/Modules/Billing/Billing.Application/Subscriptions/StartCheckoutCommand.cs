using Billing.Application.Dtos;
using Billing.Domain;
using MediatR;
using SharedKernel;

namespace Billing.Application.Subscriptions;

// Plan is the paid plan the owner wants (Basic or Premium); the validator rejects anything else.
public sealed record StartCheckoutCommand(Guid UserId, SubscriptionPlan Plan) : IRequest<Result<CheckoutDto>>;
