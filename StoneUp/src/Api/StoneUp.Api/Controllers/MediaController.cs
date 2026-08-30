using System.Security.Claims;
using Media.Application.Media;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StoneUp.Api.Controllers;

[ApiController]
[Route("api/v1/media")]
[Authorize]
public sealed class MediaController(IMediator mediator) : ControllerBase
{
    // Issue a presigned URL the client PUTs the file to directly (never proxied).
    [HttpPost("presign")]
    public async Task<IActionResult> Presign(PresignRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new RequestUploadUrlCommand(CallerUserId, request.FileName, request.ContentType), ct);
        return result.ToActionResult();
    }

    // Mark the upload complete once the client's direct PUT succeeds.
    [HttpPost("{id:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new ConfirmUploadCommand(id, CallerUserId), ct);
        return result.ToActionResult();
    }

    private Guid CallerUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

public sealed record PresignRequest(string FileName, string ContentType);
