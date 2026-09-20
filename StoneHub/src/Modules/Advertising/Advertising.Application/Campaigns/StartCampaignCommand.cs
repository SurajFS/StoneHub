using MediatR;
using SharedKernel;

namespace Advertising.Application.Campaigns;

public sealed record StartCampaignCommand(Guid SellerId, Guid ProductId) : IRequest<Result<Guid>>;
