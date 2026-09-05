using Identity.Application.Dtos;
using MediatR;
using SharedKernel;

namespace Identity.Application.Auth;

public sealed record GetMeQuery(Guid UserId) : IRequest<Result<MeDto>>;
