using Identity.Application.Dtos;
using MediatR;
using SharedKernel;

namespace Identity.Application.Auth;

public sealed record LoginCommand(string Email, string Password) : IRequest<Result<AuthResultDto>>;
