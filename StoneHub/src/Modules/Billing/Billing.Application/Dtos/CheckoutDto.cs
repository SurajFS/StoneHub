namespace Billing.Application.Dtos;

// The gateway's hosted page where the owner authorises the auto-renewing Premium subscription.
public sealed record CheckoutDto(string CheckoutUrl);
