using Identity.Application.Dtos;
using MediatR;
using SharedKernel;

namespace Identity.Application.Auth;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthResultDto>>;
