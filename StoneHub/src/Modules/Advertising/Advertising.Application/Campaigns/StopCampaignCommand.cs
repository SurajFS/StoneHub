using MediatR;
using SharedKernel;

namespace Advertising.Application.Campaigns;

public sealed record StopCampaignCommand(Guid SellerId, Guid CampaignId) : IRequest<Result>;
