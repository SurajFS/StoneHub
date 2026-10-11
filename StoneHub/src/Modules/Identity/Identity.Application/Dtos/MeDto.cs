namespace Identity.Application.Dtos;

// Current user + their editable profile, for session hydration, headers, and the profile
// screen/edit form. Profile fields are null if the profile isn't found; Phone/WhatsApp are null
// for buyers (they don't carry those). City/State are display names (resolved from Address
// when present). Address is null for profiles that haven't been given a structured address yet.
public sealed record MeDto(
    Guid UserId,
    string Email,
    string Role,
    string? Name,
    string? City,
    string? State,
    string? Phone,
    string? WhatsAppNumber,
    string? AvatarUrl,
    ProfileAddressDto? Address);
