namespace Identity.Application.Dtos;

// Current user + their editable profile, for session hydration, headers, and the profile
// screen/edit form. Profile fields are null if the profile isn't found; State/Phone/WhatsApp
// are null for buyers (they don't carry those).
public sealed record MeDto(
    Guid UserId,
    string Email,
    string Role,
    string? Name,
    string? City,
    string? State,
    string? Phone,
    string? WhatsAppNumber,
    string? AvatarUrl);
