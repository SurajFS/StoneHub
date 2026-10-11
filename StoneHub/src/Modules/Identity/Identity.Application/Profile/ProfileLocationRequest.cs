using Identity.Domain;

namespace Identity.Application.Profile;

// Everything needed to turn a signup / profile-edit request into the profile's location.
// Current* describe the stored profile (all null at signup).
public sealed record ProfileLocationRequest(
    string Role,
    AddressRequirement Requirement,
    ProfileAddressInput? Address,
    string? LegacyCity,
    string? LegacyState,
    string? CurrentCity,
    string? CurrentState,
    Address? CurrentAddress);
