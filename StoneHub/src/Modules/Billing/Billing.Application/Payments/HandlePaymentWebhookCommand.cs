using MediatR;
using SharedKernel;

namespace Billing.Application.Payments;

// Payload is the raw request body: the signature is computed over it byte for byte.
public sealed record HandlePaymentWebhookCommand(string EventId, string Payload, string Signature)
    : IRequest<Result<PaymentWebhookOutcome>>;
