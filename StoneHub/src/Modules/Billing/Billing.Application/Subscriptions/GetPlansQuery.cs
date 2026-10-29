using Billing.Application.Dtos;
using MediatR;

namespace Billing.Application.Subscriptions;

public sealed record GetPlansQuery(Guid UserId) : IRequest<IReadOnlyList<PlanOfferDto>>;
