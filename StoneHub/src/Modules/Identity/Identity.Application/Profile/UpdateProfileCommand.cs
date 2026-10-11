using Identity.Application.Dtos;
using MediatR;
using SharedKernel;

namespace Identity.Application.Profile;

// Updates the caller's own profile. Which fields apply depends on their role (buyers have no
// phone/WhatsApp). Address is the structured location. City/State are the legacy free-text
// fields from installed APKs, used only when Address is absent. Returns the refreshed profile.
public sealed record UpdateProfileCommand(
    Guid UserId,
    string Name,
    ProfileAddressInput? Address,
    string? City,
    string? State,
    string? Phone,
    string? WhatsAppNumber,
    string? AvatarUrl) : IRequest<Result<MeDto>>;
