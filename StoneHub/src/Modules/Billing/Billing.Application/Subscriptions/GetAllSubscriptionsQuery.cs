using Billing.Application.Dtos;
using MediatR;
using SharedKernel;

namespace Billing.Application.Subscriptions;

// Admin-only: every seller/wholesaler's subscription, most recently created first.
public sealed record GetAllSubscriptionsQuery(int Page, int PageSize) : IRequest<Result<PagedResult<AdminSubscriptionRowDto>>>;
