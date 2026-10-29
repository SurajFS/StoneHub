namespace Billing.Application.Dtos;

// A paid plan the caller can subscribe to. Prices are in paise (₹1 = 100). FirstMonthPricePaise is
// set only when the launch offer runs for this plan and the caller has never paid before.
public sealed record PlanOfferDto(string Plan, int MonthlyPricePaise, int? FirstMonthPricePaise);
