namespace Billing.Application.Payments;

// What a webhook delivery did, so the transport layer can log it. All of these are acknowledged
// with a 2xx: retrying wouldn't change the outcome.
public enum PaymentWebhookOutcome
{
    Applied,
    Duplicate,
    Ignored,
    UnknownSubscription
}
