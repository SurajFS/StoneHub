using SharedKernel;

namespace Identity.Domain;

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

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return City;
        yield return State;
    }
}
