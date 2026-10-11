namespace Locations.Infrastructure.Persistence;

// India's 28 states and 8 union territories with their ISO 3166-2:IN codes, including the
// 2023-11-23 changes (CT→CG, OR→OD, TG→TS, UT→UK). Names follow the official short forms, and
// the postal directory's StateName column is matched against them (see PostalDirectoryParser).
internal static class RegionSeed
{
    public sealed record Entry(string Code, string Name);

    public static readonly IReadOnlyList<Entry> India =
    [
        new("IN-AN", "Andaman and Nicobar Islands"),
        new("IN-AP", "Andhra Pradesh"),
        new("IN-AR", "Arunachal Pradesh"),
        new("IN-AS", "Assam"),
        new("IN-BR", "Bihar"),
        new("IN-CH", "Chandigarh"),
        new("IN-CG", "Chhattisgarh"),
        new("IN-DH", "Dadra and Nagar Haveli and Daman and Diu"),
        new("IN-DL", "Delhi"),
        new("IN-GA", "Goa"),
        new("IN-GJ", "Gujarat"),
        new("IN-HR", "Haryana"),
        new("IN-HP", "Himachal Pradesh"),
        new("IN-JK", "Jammu and Kashmir"),
        new("IN-JH", "Jharkhand"),
        new("IN-KA", "Karnataka"),
        new("IN-KL", "Kerala"),
        new("IN-LA", "Ladakh"),
        new("IN-LD", "Lakshadweep"),
        new("IN-MP", "Madhya Pradesh"),
        new("IN-MH", "Maharashtra"),
        new("IN-MN", "Manipur"),
        new("IN-ML", "Meghalaya"),
        new("IN-MZ", "Mizoram"),
        new("IN-NL", "Nagaland"),
        new("IN-OD", "Odisha"),
        new("IN-PY", "Puducherry"),
        new("IN-PB", "Punjab"),
        new("IN-RJ", "Rajasthan"),
        new("IN-SK", "Sikkim"),
        new("IN-TN", "Tamil Nadu"),
        new("IN-TS", "Telangana"),
        new("IN-TR", "Tripura"),
        new("IN-UP", "Uttar Pradesh"),
        new("IN-UK", "Uttarakhand"),
        new("IN-WB", "West Bengal"),
    ];
}
