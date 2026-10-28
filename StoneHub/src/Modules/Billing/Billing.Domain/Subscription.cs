using SharedKernel;

namespace Billing.Domain;

// One row per Seller/Wholesaler user, created automatically on registration (Free). Premium is
// granted either by a payment-gateway charge (auto-renewing, see ApplyGatewayCharge) or by an
// admin activation for manual / offline payments (see ActivatePremium).
public sealed class Subscription : AggregateRoot<Guid>
{
    // How long past a paid period's end Premium is kept while the renewal charge's webhook is
    // on its way. Without it, the expiry sweep could downgrade a renewing seller (stopping their
    // campaigns) minutes before the charge lands.
    public static readonly TimeSpan RenewalGracePeriod = TimeSpan.FromDays(2);

    public Guid UserId { get; private set; }
    public SubscriptionPlan Plan { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset? ExpiryDate { get; private set; }
    public string? PaymentNotes { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    // The gateway's recurring subscription this account is linked to (Razorpay "sub_..."), set
    // when checkout starts and kept while it renews.
    public string? GatewaySubscriptionId { get; private set; }

    // True while the gateway will charge again at the end of the period. False once the owner
    // cancels, the gateway gives up on a failed charge, or Premium came from an admin.
    public bool WillRenew { get; private set; }

    // A started-but-unpaid checkout, reused until it expires so repeated taps on "Upgrade"
    // don't create a new gateway subscription each time.
    public string? CheckoutUrl { get; private set; }
    public DateTimeOffset? CheckoutExpiresAt { get; private set; }

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

    public bool IsPremiumActive => Plan == SubscriptionPlan.Premium && Status == SubscriptionStatus.Active;

    // Admin activation for payments taken outside the gateway. paymentNotes is a free-text
    // record of how/what was paid. Doesn't renew on its own.
    public void ActivatePremium(DateTimeOffset expiryDate, string? paymentNotes)
    {
        Plan = SubscriptionPlan.Premium;
        Status = SubscriptionStatus.Active;
        StartDate = DateTimeOffset.UtcNow;
        ExpiryDate = expiryDate;
        PaymentNotes = paymentNotes;
        WillRenew = false;
        Raise(new SubscriptionActivatedEvent(UserId, expiryDate));
    }

    // Links a freshly created gateway checkout. Refused while Premium is active, so an owner
    // can't start (and pay for) a second period on top of the current one.
    public Result StartCheckout(string gatewaySubscriptionId, string checkoutUrl, DateTimeOffset expiresAt)
    {
        if (IsPremiumActive)
            return Result.Conflict("This account is already on Premium.");

        GatewaySubscriptionId = gatewaySubscriptionId;
        CheckoutUrl = checkoutUrl;
        CheckoutExpiresAt = expiresAt;
        WillRenew = false;
        return Result.Success();
    }

    public string? ReusableCheckoutUrl(DateTimeOffset now) =>
        !IsPremiumActive && CheckoutExpiresAt > now ? CheckoutUrl : null;

    // A successful gateway charge (first payment or renewal): Premium until the paid period ends,
    // plus the renewal grace. Raises SubscriptionActivatedEvent only on the Free → Premium
    // transition; a renewal just extends the expiry. The charged gateway subscription becomes
    // the linked one (it may differ if the owner started a second checkout).
    public void ApplyGatewayCharge(string gatewaySubscriptionId, DateTimeOffset periodEnd)
    {
        var expiry = periodEnd + RenewalGracePeriod;
        var wasPremium = IsPremiumActive;

        GatewaySubscriptionId = gatewaySubscriptionId;
        Plan = SubscriptionPlan.Premium;
        Status = SubscriptionStatus.Active;
        // Webhooks can arrive out of order: an older period's charge never shortens Premium.
        ExpiryDate = wasPremium && ExpiryDate > expiry ? ExpiryDate : expiry;
        PaymentNotes = null;
        WillRenew = true;
        CheckoutUrl = null;
        CheckoutExpiresAt = null;

        if (!wasPremium)
        {
            StartDate = DateTimeOffset.UtcNow;
            Raise(new SubscriptionActivatedEvent(UserId, expiry));
        }
    }

    // The gateway won't charge again (owner cancelled at cycle end, retries exhausted, or all
    // cycles done). Premium stays until ExpiryDate; the expiry sweep downgrades after that.
    public void StopRenewal() => WillRenew = false;

    // Cancelling reverts to Free immediately, same as a natural expiry.
    public void Cancel()
    {
        if (Plan != SubscriptionPlan.Premium)
            return;

        Plan = SubscriptionPlan.Free;
        Status = SubscriptionStatus.Cancelled;
        ExpiryDate = null;
        PaymentNotes = null;
        WillRenew = false;
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
        WillRenew = false;
        Raise(new SubscriptionExpiredEvent(UserId));
        return true;
    }
}
