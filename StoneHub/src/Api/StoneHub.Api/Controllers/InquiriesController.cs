using System.Security.Claims;
using Inquiries.Application;
using Inquiries.Application.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StoneHub.Api.Controllers;

[ApiController]
[Route("api/v1/inquiries")]
public sealed class InquiriesController(IMediator mediator) : ControllerBase
{
    // A seller sends a bulk inquiry about a wholesaler's listing.
    [Authorize(Roles = "Seller")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateInquiryRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new CreateInquiryCommand(CallerUserId, request.ProductId, request.Quantity, request.Message), ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Sent), null, result.Value)
            : result.ToActionResult();
    }

    // Wholesaler: inquiries received from sellers.
    [Authorize(Roles = "Wholesaler")]
    [HttpGet("received")]
    public async Task<ActionResult<IReadOnlyList<InquiryDto>>> Received(CancellationToken ct)
    {
        var results = await mediator.Send(new GetReceivedInquiriesQuery(CallerUserId), ct);
        return Ok(results);
    }

    // Seller: inquiries they have sent.
    [Authorize(Roles = "Seller")]
    [HttpGet("sent")]
    public async Task<ActionResult<IReadOnlyList<InquiryDto>>> Sent(CancellationToken ct)
    {
        var results = await mediator.Send(new GetSentInquiriesQuery(CallerUserId), ct);
        return Ok(results);
    }

    [Authorize(Roles = "Wholesaler")]
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateInquiryStatusRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateInquiryStatusCommand(id, CallerUserId, request.Status), ct);
        return result.IsSuccess ? NoContent() : result.ToActionResult();
    }

    private Guid CallerUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

public sealed record CreateInquiryRequest(Guid ProductId, decimal Quantity, string? Message);

public sealed record UpdateInquiryStatusRequest(string Status);
