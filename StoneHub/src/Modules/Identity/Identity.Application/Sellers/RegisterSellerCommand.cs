using Identity.Application.Dtos;
using Identity.Application.Profile;
using MediatR;
using SharedKernel;

namespace Identity.Application.Sellers;

// Address is the structured location. City/State are the legacy free-text fields still sent by
// installed APKs. They're used only when Address is absent.
public sealed record RegisterSellerCommand(
    string Email,
    string Password,
    string CompanyName,
    ProfileAddressInput? Address,
    string? City,
    string? State,
    string Phone,
    string? WhatsAppNumber) : IRequest<Result<AuthResultDto>>;
