using Identity.Application.Dtos;
using MediatR;
using SharedKernel;

namespace Identity.Application.Wholesalers;

public sealed record RegisterWholesalerCommand(
    string Email,
    string Password,
    string BusinessName,
    string City,
    string State,
    string Phone,
    string? WhatsAppNumber) : IRequest<Result<AuthResultDto>>;
