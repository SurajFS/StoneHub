using Locations.Application.Dtos;
using MediatR;
using SharedKernel;

namespace Locations.Application.Queries;

// Resolves a postal code to the localities it serves (with district + region) for address autofill.
public sealed record LookupPostalCodeQuery(string CountryCode, string PostalCode)
    : IRequest<Result<PostalCodeLookupDto>>;
