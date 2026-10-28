using Billing.Application.Dtos;
using MediatR;
using SharedKernel;

namespace Billing.Application.Subscriptions;

public sealed record StartPremiumCheckoutCommand(Guid UserId) : IRequest<Result<CheckoutDto>>;
