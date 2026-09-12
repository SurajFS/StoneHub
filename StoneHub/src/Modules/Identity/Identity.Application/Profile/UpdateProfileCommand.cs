using Identity.Application.Dtos;
using MediatR;
using SharedKernel;

namespace Identity.Application.Profile;

// Updates the caller's own profile. Which fields apply depends on their role (buyers have no
// state/phone/WhatsApp). Returns the refreshed profile so the client can update in place.
public sealed record UpdateProfileCommand(
    Guid UserId,
    string Name,
    string City,
    string? State,
    string? Phone,
    string? WhatsAppNumber,
    string? AvatarUrl) : IRequest<Result<MeDto>>;
