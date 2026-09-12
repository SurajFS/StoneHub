namespace Messaging.Domain;

// Which role pairs may open a chat. Symmetric. A seller bridges buyers and wholesalers; buyers
// and wholesalers cannot chat each other, and same-role chat is not allowed.
public static class ChatPolicy
{
    public const string Buyer = "Buyer";
    public const string Seller = "Seller";
    public const string Wholesaler = "Wholesaler";

    public static bool CanConverse(string roleOne, string roleTwo) =>
        IsPair(roleOne, roleTwo, Buyer, Seller) ||
        IsPair(roleOne, roleTwo, Seller, Wholesaler);

    private static bool IsPair(string a, string b, string x, string y) =>
        (Matches(a, x) && Matches(b, y)) || (Matches(a, y) && Matches(b, x));

    private static bool Matches(string role, string expected) =>
        string.Equals(role, expected, StringComparison.OrdinalIgnoreCase);
}
