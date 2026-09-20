using SharedKernel;

namespace Billing.Domain;

// One row per Seller/Wholesaler user, created automatically on registration (Free) and upgraded
// by an admin-triggered activation for now (no payment gateway wired yet — see ActivatePremium).
public sealed class Subscription : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public SubscriptionPlan Plan { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset? ExpiryDate { get; private set; }
    public string? PaymentNotes { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Subscription() { }

    private Subscription(Guid id, Guid userId) : base(id)
    {
        UserId = userId;
        Plan = SubscriptionPlan.Free;
        Status = SubscriptionStatus.Active;
        StartDate = DateTimeOffset.UtcNow;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static Subscription CreateFree(Guid userId) => new(Guid.NewGuid(), userId);

    // Stand-in for a real payment gateway: an admin marks the subscription Premium until
    // expiryDate. paymentNotes is a free-text record of how/what was paid (manual for now).
    public void ActivatePremium(DateTimeOffset expiryDate, string? paymentNotes)
    {
        Plan = SubscriptionPlan.Premium;
        Status = SubscriptionStatus.Active;
        StartDate = DateTimeOffset.UtcNow;
        ExpiryDate = expiryDate;
        PaymentNotes = paymentNotes;
        Raise(new SubscriptionActivatedEvent(UserId, expiryDate));
    }

    // Cancelling reverts to Free immediately, same as a natural expiry.
    public void Cancel()
    {
        if (Plan != SubscriptionPlan.Premium)
            return;

        Plan = SubscriptionPlan.Free;
        Status = SubscriptionStatus.Cancelled;
        ExpiryDate = null;
        PaymentNotes = null;
        Raise(new SubscriptionCancelledEvent(UserId));
    }

    // Called by the daily expiry sweep. Returns whether it actually changed state, so the
    // caller only persists/logs rows that were really due.
    public bool ExpireIfDue(DateTimeOffset now)
    {
        if (Plan != SubscriptionPlan.Premium || Status != SubscriptionStatus.Active)
            return false;
        if (ExpiryDate is null || ExpiryDate.Value > now)
            return false;

        Plan = SubscriptionPlan.Free;
        Status = SubscriptionStatus.Expired;
        PaymentNotes = null;
        Raise(new SubscriptionExpiredEvent(UserId));
        return true;
    }
}
