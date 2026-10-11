using Identity.Application.Dtos;
using Identity.Application.Profile;
using MediatR;
using SharedKernel;

namespace Identity.Application.Wholesalers;

// Address is the structured location. City/State are the legacy free-text fields still sent by
// installed APKs. They're used only when Address is absent.
public sealed record RegisterWholesalerCommand(
    string Email,
    string Password,
    string BusinessName,
    ProfileAddressInput? Address,
    string? City,
    string? State,
    string Phone,
    string? WhatsAppNumber) : IRequest<Result<AuthResultDto>>;
