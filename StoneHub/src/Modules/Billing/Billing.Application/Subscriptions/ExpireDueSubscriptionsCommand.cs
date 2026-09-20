using MediatR;
using SharedKernel;

namespace Billing.Application.Subscriptions;

// Automatic downgrade sweep: flips every due Premium subscription back to Free.
public sealed record ExpireDueSubscriptionsCommand : IRequest<Result<int>>;
