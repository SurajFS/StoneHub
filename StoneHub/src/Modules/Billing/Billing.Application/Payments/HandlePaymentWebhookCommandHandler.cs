using Billing.Domain;
using MediatR;
using SharedKernel;

namespace Billing.Application.Payments;

// Applies a verified gateway event to the subscription it belongs to, once. Only charges grant
// Premium; ended renewals just stop renewal, and the expiry sweep downgrades after the grace
// period, so a late or out-of-order event can't strip a paid period.
public sealed class HandlePaymentWebhookCommandHandler(
    IPaymentGateway gateway,
    ISubscriptionRepository subscriptions,
    IProcessedPaymentEventRepository processedEvents)
    : IRequestHandler<HandlePaymentWebhookCommand, Result<PaymentWebhookOutcome>>
{
    public async Task<Result<PaymentWebhookOutcome>> Handle(HandlePaymentWebhookCommand request, CancellationToken ct)
    {
        if (!gateway.IsAuthenticWebhook(request.Payload, request.Signature))
            return Result.Unauthorized<PaymentWebhookOutcome>("Invalid webhook signature.");

        if (await processedEvents.ExistsAsync(request.EventId, ct))
            return Result.Success(PaymentWebhookOutcome.Duplicate);

        var paymentEvent = gateway.ParseWebhook(request.Payload);
        if (paymentEvent is null)
            return Result.Failure<PaymentWebhookOutcome>("Unreadable webhook payload.");

        var outcome = await ApplyAsync(paymentEvent, ct);
        processedEvents.Add(ProcessedPaymentEvent.Record(request.EventId, paymentEvent.EventType));
        await subscriptions.SaveChangesAsync(ct);

        return Result.Success(outcome);
    }

    private async Task<PaymentWebhookOutcome> ApplyAsync(PaymentEvent paymentEvent, CancellationToken ct)
    {
        if (paymentEvent.Kind == PaymentEventKind.Ignored || paymentEvent.GatewaySubscriptionId is null)
            return PaymentWebhookOutcome.Ignored;

        var gatewayId = paymentEvent.GatewaySubscriptionId;
        var linked = await subscriptions.GetByGatewaySubscriptionIdAsync(gatewayId, ct);

        if (paymentEvent.Kind == PaymentEventKind.RenewalEnded)
        {
            // Only the subscription currently linked to this gateway id; an older one ending
            // must not stop a newer one's renewal.
            linked?.StopRenewal();
            return linked is null ? PaymentWebhookOutcome.UnknownSubscription : PaymentWebhookOutcome.Applied;
        }

        // A charge is money taken, so it always grants Premium: fall back to the user from the
        // checkout notes when the stored gateway id was replaced (e.g. two checkouts started).
        // An activation without a period (a future-dated start) has nothing to grant yet.
        if (paymentEvent.PeriodEnd is not { } periodEnd)
            return PaymentWebhookOutcome.Ignored;

        var subscription = linked ?? (paymentEvent.UserId is { } userId ? await subscriptions.GetByUserIdAsync(userId, ct) : null);
        if (subscription is null)
            return PaymentWebhookOutcome.UnknownSubscription;

        subscription.ApplyGatewayCharge(gatewayId, periodEnd);
        return PaymentWebhookOutcome.Applied;
    }
}
