using Billing.Domain;
using MediatR;
using SharedKernel;

namespace Billing.Application.Payments;

// Applies a verified gateway event to the subscription it belongs to, once. Only charges grant a
// plan; ended renewals just stop renewal, and the expiry sweep downgrades after the grace
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
            if (linked is null)
                return PaymentWebhookOutcome.UnknownSubscription;
            // Only the subscription currently paying: an unpaid checkout or an older subscription
            // ending must not stop the current one's renewal.
            if (linked.GatewaySubscriptionId != gatewayId)
                return PaymentWebhookOutcome.Ignored;

            linked.StopRenewal();
            return PaymentWebhookOutcome.Applied;
        }

        // An activation without a period (a future-dated start) has nothing to grant yet.
        if (paymentEvent.PeriodEnd is not { } periodEnd)
            return PaymentWebhookOutcome.Ignored;
        if (paymentEvent.GatewayPlanId is null || gateway.PlanFor(paymentEvent.GatewayPlanId) is not { } plan)
            return PaymentWebhookOutcome.UnknownPlan;

        // A charge is money taken, so it always reaches the owner: fall back to the user from the
        // checkout notes when the gateway id isn't linked any more.
        var subscription = linked ?? (paymentEvent.UserId is { } userId ? await subscriptions.GetByUserIdAsync(userId, ct) : null);
        if (subscription is null)
            return PaymentWebhookOutcome.UnknownSubscription;

        return subscription.ApplyGatewayCharge(gatewayId, plan, periodEnd)
            ? PaymentWebhookOutcome.Applied
            : PaymentWebhookOutcome.Stale;
    }
}
