namespace Billing.Application.Payments;

public enum PaymentEventKind
{
    // Not one Billing acts on (authenticated, pending, updated, …). Recorded, then acknowledged.
    Ignored,

    // A charge succeeded: Premium until PeriodEnd.
    Charged,

    // The gateway won't charge again: cancelled, halted after failed retries, or completed.
    RenewalEnded
}
