using System.Security.Claims;
using Advertising.Application.Campaigns;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StoneHub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/advertising")]
public sealed class AdvertisingController(IMediator mediator) : ControllerBase
{
    // The caller's own campaigns, most recently started first. Premium/ownership are
    // re-validated on Start, not here — this is just a read.
    [HttpGet("campaigns")]
    public async Task<IActionResult> MyCampaigns(CancellationToken ct)
    {
        var result = await mediator.Send(new GetMyCampaignsQuery(CallerUserId), ct);
        return result.ToActionResult();
    }

    [HttpPost("campaigns")]
    public async Task<IActionResult> Start(StartCampaignRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new StartCampaignCommand(CallerUserId, request.ProductId), ct);
        return result.IsSuccess
            ? Ok(new StartCampaignResponse(result.Value))
            : result.ToActionResult();
    }

    [HttpPost("campaigns/{campaignId:guid}/stop")]
    public async Task<IActionResult> Stop(Guid campaignId, CancellationToken ct)
    {
        var result = await mediator.Send(new StopCampaignCommand(CallerUserId, campaignId), ct);
        return result.IsSuccess ? NoContent() : result.ToActionResult();
    }

    private Guid CallerUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

public sealed record StartCampaignRequest(Guid ProductId);

public sealed record StartCampaignResponse(Guid CampaignId);
