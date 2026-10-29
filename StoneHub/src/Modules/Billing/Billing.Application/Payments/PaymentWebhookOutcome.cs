namespace Billing.Application.Payments;

// What a webhook delivery did, so the transport layer can log it. All of these are acknowledged
// with a 2xx: retrying wouldn't change the outcome.
public enum PaymentWebhookOutcome
{
    Applied,
    Duplicate,
    Ignored,
    UnknownSubscription,

    // A charge for a gateway plan we don't sell (misconfigured plan ids). Needs a human: money
    // was taken but no plan was granted.
    UnknownPlan,

    // An older or replaced subscription's charge that changes nothing.
    Stale
}
