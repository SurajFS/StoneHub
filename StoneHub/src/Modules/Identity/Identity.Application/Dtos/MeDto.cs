namespace Identity.Application.Dtos;

// Current user + a bit of profile (company/display name + city) for the app's session
// hydration and headers. Name/City are null if the profile isn't found.
public sealed record MeDto(Guid UserId, string Email, string Role, string? Name, string? City);
