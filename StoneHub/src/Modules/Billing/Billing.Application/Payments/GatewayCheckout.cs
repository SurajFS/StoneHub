namespace Billing.Application.Payments;

public sealed record GatewayCheckout(string GatewaySubscriptionId, string CheckoutUrl, DateTimeOffset ExpiresAt);
