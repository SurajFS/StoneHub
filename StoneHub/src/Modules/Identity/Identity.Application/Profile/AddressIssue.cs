namespace Identity.Application.Profile;

// Why a profile shows up on the address data-quality report.
public enum AddressIssue
{
    // Still free-text city/state: legacy signup, or the backfill found no exact district match.
    NoStructuredAddress,

    // Sellers / Wholesalers only: have a district but no street line (e.g. backfilled).
    MissingStreet,

    // Sellers / Wholesalers only: have a district but no postal code (e.g. backfilled).
    MissingPostalCode,
}
