using Identity.Application.Dtos;
using MediatR;
using SharedKernel;

namespace Identity.Application.Sellers;

public sealed record RegisterSellerCommand(
    string Email,
    string Password,
    string CompanyName,
    string City,
    string State,
    string Phone,
    string? WhatsAppNumber) : IRequest<Result<AuthResultDto>>;
