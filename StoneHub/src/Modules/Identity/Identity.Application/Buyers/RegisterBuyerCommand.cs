using Identity.Application.Dtos;
using Identity.Application.Profile;
using Identity.Domain;
using MediatR;
using SharedKernel;

namespace Identity.Application.Buyers;

// Address is the structured location. City is the legacy free-text field still sent by
// installed APKs. It's used only when Address is absent.
public sealed record RegisterBuyerCommand(
    string Email,
    string Password,
    string DisplayName,
    BuyerType BuyerType,
    ProfileAddressInput? Address,
    string? City) : IRequest<Result<AuthResultDto>>;
