using System.Security.Claims;
using Identity.Application.Auth;
using Identity.Application.Buyers;
using Identity.Application.Profile;
using Identity.Application.Sellers;
using Identity.Application.Wholesalers;
using Identity.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StoneHub.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register/seller")]
    public async Task<IActionResult> RegisterSeller(RegisterSellerRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new RegisterSellerCommand(
                request.Email,
                request.Password,
                request.CompanyName,
                request.City,
                request.State,
                request.Phone,
                request.WhatsAppNumber),
            ct);

        return result.ToActionResult();
    }

    [HttpPost("register/wholesaler")]
    public async Task<IActionResult> RegisterWholesaler(RegisterWholesalerRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new RegisterWholesalerCommand(
                request.Email,
                request.Password,
                request.BusinessName,
                request.City,
                request.State,
                request.Phone,
                request.WhatsAppNumber),
            ct);

        return result.ToActionResult();
    }

    [HttpPost("register/buyer")]
    public async Task<IActionResult> RegisterBuyer(RegisterBuyerRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new RegisterBuyerCommand(request.Email, request.Password, request.DisplayName, request.BuyerType, request.City),
            ct);

        return result.ToActionResult();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new LoginCommand(request.Email, request.Password), ct);
        return result.ToActionResult();
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new RefreshTokenCommand(request.RefreshToken), ct);
        return result.ToActionResult();
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new LogoutCommand(request.RefreshToken), ct);
        return result.ToActionResult();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var result = await mediator.Send(new GetMeQuery(CallerUserId), ct);
        return result.ToActionResult();
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe(UpdateProfileRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new UpdateProfileCommand(
                CallerUserId,
                request.Name,
                request.City,
                request.State,
                request.Phone,
                request.WhatsAppNumber,
                request.AvatarUrl),
            ct);

        return result.ToActionResult();
    }

    private Guid CallerUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

public sealed record RegisterSellerRequest(
    string Email,
    string Password,
    string CompanyName,
    string City,
    string State,
    string Phone,
    string? WhatsAppNumber);

public sealed record RegisterWholesalerRequest(
    string Email,
    string Password,
    string BusinessName,
    string City,
    string State,
    string Phone,
    string? WhatsAppNumber);

public sealed record RegisterBuyerRequest(string Email, string Password, string DisplayName, BuyerType BuyerType, string City);

public sealed record UpdateProfileRequest(
    string Name,
    string City,
    string? State,
    string? Phone,
    string? WhatsAppNumber,
    string? AvatarUrl);

public sealed record LoginRequest(string Email, string Password);

public sealed record RefreshRequest(string RefreshToken);
