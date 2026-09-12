using System.Security.Claims;
using MediatR;
using Messaging.Application;
using Messaging.Application.Dtos;
using Messaging.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StoneHub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/conversations")]
public sealed class ConversationsController(IMediator mediator) : ControllerBase
{
    // Open (or fetch the existing) chat thread with a product's owner.
    [HttpPost]
    public async Task<IActionResult> Start(StartConversationRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new StartConversationCommand(CallerUserId, request.ProductId), ct);
        return result.IsSuccess
            ? Ok(new StartConversationResponse(result.Value))
            : result.ToActionResult();
    }

    // The caller's chat list, most recent first.
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ConversationDto>>> List(CancellationToken ct)
    {
        var results = await mediator.Send(new GetConversationsQuery(CallerUserId), ct);
        return Ok(results);
    }

    // Thread messages. `since` (ISO timestamp) returns only newer messages, for polling.
    [HttpGet("{id:guid}/messages")]
    public async Task<IActionResult> Messages(Guid id, [FromQuery] DateTimeOffset? since, CancellationToken ct)
    {
        var result = await mediator.Send(new GetMessagesQuery(id, CallerUserId, since), ct);
        return result.ToActionResult();
    }

    [HttpPost("{id:guid}/messages")]
    public async Task<IActionResult> Send(Guid id, SendMessageRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new SendMessageCommand(id, CallerUserId, request.Kind, request.Body, request.MediaUrl), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Messages), new { id }, result.Value)
            : result.ToActionResult();
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new MarkConversationReadCommand(id, CallerUserId), ct);
        return result.IsSuccess ? NoContent() : result.ToActionResult();
    }

    private Guid CallerUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

public sealed record StartConversationRequest(Guid ProductId);

public sealed record StartConversationResponse(Guid ConversationId);

public sealed record SendMessageRequest(MessageKind Kind, string? Body, string? MediaUrl);
