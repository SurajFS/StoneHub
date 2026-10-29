namespace Billing.Domain;

// Stored as its name (see SubscriptionConfiguration), so adding a tier needs no data migration.
public enum SubscriptionPlan
{
    // Sellers/wholesalers can search and browse, but not add listings or read/send messages.
    Free,

    // Add listings and chat.
    Basic,

    // Everything in Basic, plus higher search placement and Sponsored listings.
    Premium
}
