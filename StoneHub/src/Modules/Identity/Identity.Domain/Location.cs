using SharedKernel;

namespace Identity.Domain;

// The display location ("Jaipur", "Rajasthan"). For a structured Address it holds the resolved
// names (locality, or district when there's no locality, plus the state). For a legacy profile
// it holds the free text the user typed.
public sealed class Location : ValueObject
{
    public string City { get; }
    public string State { get; }

    private Location(string city, string state)
    {
        City = city;
        State = state;
    }

    public static Location Create(string city, string state)
    {
        if (string.IsNullOrWhiteSpace(city)) throw new ArgumentException("City is required.", nameof(city));
        if (string.IsNullOrWhiteSpace(state)) throw new ArgumentException("State is required.", nameof(state));
        return new Location(city.Trim(), state.Trim());
    }

    // "City, State": the label shown on listings and used by location search.
    public static string Format(string city, string? state) =>
        string.IsNullOrWhiteSpace(state) ? city : $"{city}, {state}";

    public string DisplayName => Format(City, State);

    // Case-insensitive, because legacy clients resend whatever the user typed.
    public bool SameAs(string city, string? state) =>
        string.Equals(City, city.Trim(), StringComparison.OrdinalIgnoreCase)
        && string.Equals(State, state?.Trim(), StringComparison.OrdinalIgnoreCase);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return City;
        yield return State;
    }
}
