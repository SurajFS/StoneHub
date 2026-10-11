namespace Identity.Application.Profile;

public enum AddressRequirement
{
    // Buyers: state + district are enough.
    DistrictOnly,

    // Sellers / Wholesalers: also a street line and a postal code (decision 2026-10-11).
    StreetAndPostalCode,
}
