using Identity.Application.Auth;
using Identity.Application.Buyers;
using Identity.Application.Sellers;
using Identity.Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace StoneUp.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register/seller")]
    public async Task<IActionResult> RegisterSeller(RegisterSellerRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new RegisterSellerCommand(request.Email, request.Password, request.CompanyName, request.City, request.State),
            ct);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("register/buyer")]
    public async Task<IActionResult> RegisterBuyer(RegisterBuyerRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new RegisterBuyerCommand(request.Email, request.Password, request.DisplayName, request.BuyerType, request.City),
            ct);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new LoginCommand(request.Email, request.Password), ct);
        return result.IsSuccess ? Ok(result.Value) : Unauthorized(result.Error);
    }
}

public sealed record RegisterSellerRequest(string Email, string Password, string CompanyName, string City, string State);

public sealed record RegisterBuyerRequest(string Email, string Password, string DisplayName, BuyerType BuyerType, string City);

public sealed record LoginRequest(string Email, string Password);
