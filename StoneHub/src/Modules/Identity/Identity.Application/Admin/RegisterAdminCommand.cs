using Identity.Application.Dtos;
using MediatR;
using SharedKernel;

namespace Identity.Application.Admin;

// No profile entity — an admin isn't a marketplace participant, just a role. The bootstrap
// secret gate lives at the controller (transport boundary), not here.
public sealed record RegisterAdminCommand(string Email, string Password, string DisplayName) : IRequest<Result<AuthResultDto>>;
