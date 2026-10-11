namespace Identity.Application.Profile;

// One profile's address facts, as read for the data-quality report. Init-only properties
// (not a positional record) so EF can project all three profile tables into it and UNION them.
public sealed record ProfileAddressRow
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string? State { get; init; }
    public bool HasAddress { get; init; }
    public bool HasStreet { get; init; }
    public bool HasPostalCode { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
