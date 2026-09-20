using System.Security.Claims;
using Billing.Application.Subscriptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StoneHub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/billing")]
public sealed class BillingController(IMediator mediator) : ControllerBase
{
    // The caller's own subscription (Free or Premium, with status/dates).
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var result = await mediator.Send(new GetMySubscriptionQuery(CallerUserId), ct);
        return result.ToActionResult();
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/subscriptions")]
    public async Task<IActionResult> ListAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetAllSubscriptionsQuery(page, pageSize), ct);
        return result.ToActionResult();
    }

    // Stand-in for a real payment gateway checkout (see task.md — deferred for now).
    [Authorize(Roles = "Admin")]
    [HttpPost("admin/subscriptions/{userId:guid}/activate-premium")]
    public async Task<IActionResult> ActivatePremium(Guid userId, ActivatePremiumRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new ActivatePremiumCommand(userId, request.ExpiryDate, request.PaymentNotes), ct);
        return result.ToActionResult();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/subscriptions/{userId:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid userId, CancellationToken ct)
    {
        var result = await mediator.Send(new CancelSubscriptionCommand(userId), ct);
        return result.ToActionResult();
    }

    private Guid CallerUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

public sealed record ActivatePremiumRequest(DateTimeOffset ExpiryDate, string? PaymentNotes);
