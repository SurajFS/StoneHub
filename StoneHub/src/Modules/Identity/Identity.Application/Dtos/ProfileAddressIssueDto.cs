using Identity.Application.Profile;

namespace Identity.Application.Dtos;

// One row of the admin address data-quality report. City/State are what the profile shows today.
public sealed record ProfileAddressIssueDto(
    Guid UserId,
    string Email,
    string Role,
    string Name,
    string City,
    string? State,
    IReadOnlyList<AddressIssue> Issues);
