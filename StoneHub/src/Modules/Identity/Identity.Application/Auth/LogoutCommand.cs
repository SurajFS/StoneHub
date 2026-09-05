using MediatR;
using SharedKernel;

namespace Identity.Application.Auth;

public sealed record LogoutCommand(string RefreshToken) : IRequest<Result>;
