using Identity.Domain;

namespace Identity.Application.Profile;

// The display City/State to store, plus the structured Address (null on the legacy path when
// it can't be kept).
public sealed record ResolvedProfileLocation(string City, string? State, Address? Address);
