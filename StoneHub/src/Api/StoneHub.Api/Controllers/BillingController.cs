using System.Net.Mime;
using System.Security.Claims;
using Billing.Application.Payments;
using Billing.Application.Subscriptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StoneHub.Api.RateLimiting;

namespace StoneHub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/billing")]
public sealed class BillingController(IMediator mediator, ILogger<BillingController> logger) : ControllerBase
{
    private const string OwnerRoles = "Seller,Wholesaler";

    // The caller's own subscription (Free or Premium, with status/dates).
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var result = await mediator.Send(new GetMySubscriptionQuery(CallerUserId), ct);
        return result.ToActionResult();
    }

    // A hosted Razorpay page to authorise auto-renewing Premium. Premium itself starts when the
    // charge webhook arrives, not here.
    [Authorize(Roles = OwnerRoles)]
    [HttpPost("checkout")]
    public async Task<IActionResult> StartCheckout(CancellationToken ct)
    {
        var result = await mediator.Send(new StartPremiumCheckoutCommand(CallerUserId), ct);
        return result.ToActionResult();
    }

    // Stop auto-renewal; Premium continues to the end of the paid period.
    [Authorize(Roles = OwnerRoles)]
    [HttpPost("me/cancel")]
    public async Task<IActionResult> CancelRenewal(CancellationToken ct)
    {
        var result = await mediator.Send(new CancelPremiumRenewalCommand(CallerUserId), ct);
        return result.IsSuccess ? NoContent() : result.ToActionResult();
    }

    // Razorpay → us. Authenticated by the HMAC signature over the raw body, not a JWT. Any 2xx
    // acknowledges; anything else makes Razorpay retry.
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.PaymentWebhook)]
    // Razorpay always posts JSON. Any other type (e.g. a form) would be consumed by MVC's form
    // binding before the raw body is read, so it's refused with 415 instead of failing oddly.
    [Consumes(MediaTypeNames.Application.Json)]
    [HttpPost("webhooks/razorpay")]
    public async Task<IActionResult> RazorpayWebhook(
        [FromHeader(Name = "X-Razorpay-Signature")] string? signature,
        [FromHeader(Name = "X-Razorpay-Event-Id")] string? eventId,
        CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(ct);

        var result = await mediator.Send(new HandlePaymentWebhookCommand(eventId ?? string.Empty, payload, signature ?? string.Empty), ct);
        if (result.IsFailure)
        {
            logger.LogWarning("Razorpay webhook {EventId} rejected: {ErrorType}", eventId, result.ErrorType);
            return result.ToActionResult();
        }

        logger.LogInformation("Razorpay webhook {EventId} handled: {Outcome}", eventId, result.Value);
        return Ok();
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
