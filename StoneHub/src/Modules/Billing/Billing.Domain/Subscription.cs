using SharedKernel;

namespace Billing.Domain;

// One row per Seller/Wholesaler user, created automatically on registration (Free). A paid plan
// (Basic or Premium) is granted either by a payment-gateway charge (auto-renewing, see
// ApplyGatewayCharge) or by an admin activation for manual / offline payments (see ActivatePlan).
// Changing paid plan = stop renewal, then check out the other plan (the gateway can't change the
// plan of a UPI / e-mandate subscription in place).
public sealed class Subscription : AggregateRoot<Guid>
{
    // How long past a paid period's end the plan is kept while the renewal charge's webhook is
    // on its way. Without it, the expiry sweep could downgrade a renewing owner (stopping their
    // campaigns) minutes before the charge lands.
    public static readonly TimeSpan RenewalGracePeriod = TimeSpan.FromDays(2);

    public Guid UserId { get; private set; }
    public SubscriptionPlan Plan { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset? ExpiryDate { get; private set; }
    public string? PaymentNotes { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    // The gateway subscription the current paid period came from (Razorpay "sub_..."), kept while
    // it renews. Only a charge makes a gateway subscription current.
    public string? GatewaySubscriptionId { get; private set; }

    // True while the gateway will charge again at the end of the period. False once the owner
    // stops it, the gateway gives up on a failed charge, or the plan came from an admin.
    public bool WillRenew { get; private set; }

    // A started-but-unpaid checkout, reused until it expires so repeated taps on "Subscribe"
    // don't create a new gateway subscription each time.
    public string? CheckoutGatewaySubscriptionId { get; private set; }
    public SubscriptionPlan? CheckoutPlan { get; private set; }
    public string? CheckoutUrl { get; private set; }
    public DateTimeOffset? CheckoutExpiresAt { get; private set; }

    // Whether a gateway charge ever succeeded for this account. The launch discount is for
    // first-time subscribers only.
    public bool HasEverPaid { get; private set; }

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

    public bool IsPaidActive => Plan != SubscriptionPlan.Free && Status == SubscriptionStatus.Active;

    public bool IsPremiumActive => Plan == SubscriptionPlan.Premium && Status == SubscriptionStatus.Active;

    // A new checkout would overlap a period the gateway is still going to charge for, so it waits
    // until renewal is stopped. Once stopped, the owner may subscribe to either plan straight away.
    public bool CanStartCheckout => !IsPaidActive || !WillRenew;

    // Admin activation for payments taken outside the gateway. paymentNotes is a free-text record
    // of how/what was paid. Doesn't renew on its own.
    public Result ActivatePlan(SubscriptionPlan plan, DateTimeOffset expiryDate, string? paymentNotes)
    {
        if (plan == SubscriptionPlan.Free)
            return Result.Failure("Choose a paid plan to activate.");

        var previous = PaidPlanOrNull;
        Plan = plan;
        Status = SubscriptionStatus.Active;
        StartDate = DateTimeOffset.UtcNow;
        ExpiryDate = expiryDate;
        PaymentNotes = paymentNotes;
        WillRenew = false;
        RaisePlanEvent(previous, expiryDate);
        return Result.Success();
    }

    // Links a freshly created gateway checkout for a paid plan.
    public Result StartCheckout(SubscriptionPlan plan, string gatewaySubscriptionId, string checkoutUrl, DateTimeOffset expiresAt)
    {
        if (plan == SubscriptionPlan.Free)
            return Result.Failure("Choose a paid plan to subscribe to.");
        if (!CanStartCheckout)
            return Result.Conflict("Your current plan renews automatically. Stop auto-renewal first, then subscribe to the other plan.");

        CheckoutGatewaySubscriptionId = gatewaySubscriptionId;
        CheckoutPlan = plan;
        CheckoutUrl = checkoutUrl;
        CheckoutExpiresAt = expiresAt;
        return Result.Success();
    }

    public string? ReusableCheckoutUrl(SubscriptionPlan plan, DateTimeOffset now) =>
        CanStartCheckout && CheckoutPlan == plan && CheckoutExpiresAt > now ? CheckoutUrl : null;

    // A successful gateway charge (first payment or renewal): the charged plan until the paid
    // period ends, plus the renewal grace. A charge is money taken, so it always grants the plan,
    // except a stale one: an older or duplicate charge of the current subscription never changes
    // anything, and neither does a late charge from a subscription that has since been replaced.
    // Returns false for those stale charges, so the caller can log them.
    public bool ApplyGatewayCharge(string gatewaySubscriptionId, SubscriptionPlan plan, DateTimeOffset periodEnd)
    {
        if (plan == SubscriptionPlan.Free)
            throw new ArgumentOutOfRangeException(nameof(plan), "A gateway charge is always for a paid plan.");

        var expiry = periodEnd + RenewalGracePeriod;
        var isCurrent = gatewaySubscriptionId == GatewaySubscriptionId;
        var isCheckout = gatewaySubscriptionId == CheckoutGatewaySubscriptionId;
        var coveredAlready = IsPaidActive && ExpiryDate >= expiry;
        if (coveredAlready && (isCurrent || !isCheckout))
            return false;

        var previous = PaidPlanOrNull;
        if (!isCurrent)
        {
            // A new subscription replaces the old one, whose renewal was already stopped.
            GatewaySubscriptionId = gatewaySubscriptionId;
            StartDate = DateTimeOffset.UtcNow;
        }
        if (isCheckout)
            ClearCheckout();

        Plan = plan;
        Status = SubscriptionStatus.Active;
        ExpiryDate = expiry;
        PaymentNotes = null;
        WillRenew = true;
        HasEverPaid = true;
        RaisePlanEvent(previous, expiry);
        return true;
    }

    // The gateway won't charge again (owner stopped it, retries exhausted, or all cycles done).
    // The plan stays until ExpiryDate; the expiry sweep downgrades after that.
    public void StopRenewal() => WillRenew = false;

    // Cancelling reverts to Free immediately, same as a natural expiry.
    public void Cancel()
    {
        if (Plan == SubscriptionPlan.Free)
            return;

        RevertToFree(SubscriptionStatus.Cancelled);
        ExpiryDate = null;
        Raise(new SubscriptionCancelledEvent(UserId));
    }

    // Called by the daily expiry sweep. Returns whether it actually changed state, so the
    // caller only persists/logs rows that were really due.
    public bool ExpireIfDue(DateTimeOffset now)
    {
        if (!IsPaidActive)
            return false;
        if (ExpiryDate is null || ExpiryDate.Value > now)
            return false;

        RevertToFree(SubscriptionStatus.Expired);
        Raise(new SubscriptionExpiredEvent(UserId));
        return true;
    }

    private SubscriptionPlan? PaidPlanOrNull => IsPaidActive ? Plan : null;

    // Free → paid is an activation; Basic ↔ Premium is a plan change; same plan again is quiet.
    private void RaisePlanEvent(SubscriptionPlan? previousPaidPlan, DateTimeOffset expiry)
    {
        if (previousPaidPlan is null)
            Raise(new SubscriptionActivatedEvent(UserId, expiry, Plan));
        else if (previousPaidPlan != Plan)
            Raise(new SubscriptionPlanChangedEvent(UserId, Plan));
    }

    private void RevertToFree(SubscriptionStatus status)
    {
        Plan = SubscriptionPlan.Free;
        Status = status;
        PaymentNotes = null;
        WillRenew = false;
    }

    private void ClearCheckout()
    {
        CheckoutGatewaySubscriptionId = null;
        CheckoutPlan = null;
        CheckoutUrl = null;
        CheckoutExpiresAt = null;
    }
}
