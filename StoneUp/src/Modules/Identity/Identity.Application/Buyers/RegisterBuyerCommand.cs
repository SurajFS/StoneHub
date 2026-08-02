using Identity.Application.Dtos;
using Identity.Domain;
using MediatR;
using SharedKernel;

namespace Identity.Application.Buyers;

public sealed record RegisterBuyerCommand(
    string Email,
    string Password,
    string DisplayName,
    BuyerType BuyerType,
    string City) : IRequest<Result<AuthResultDto>>;
