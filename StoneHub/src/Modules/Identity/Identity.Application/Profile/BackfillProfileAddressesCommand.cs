using Identity.Application.Dtos;
using MediatR;
using SharedKernel;

namespace Identity.Application.Profile;

// Admin, run once after deploy: gives legacy free-text profiles a structured Address where their
// city/state matches a district exactly. Idempotent: only profiles without an Address are touched.
public sealed record BackfillProfileAddressesCommand : IRequest<Result<AddressBackfillReportDto>>;
