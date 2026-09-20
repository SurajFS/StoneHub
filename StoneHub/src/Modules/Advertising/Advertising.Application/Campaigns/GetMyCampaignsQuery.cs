using Advertising.Application.Dtos;
using MediatR;
using SharedKernel;

namespace Advertising.Application.Campaigns;

public sealed record GetMyCampaignsQuery(Guid SellerId) : IRequest<Result<IReadOnlyList<MyCampaignRowDto>>>;
