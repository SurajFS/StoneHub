using SharedKernel;

namespace Identity.Application;

public sealed record UserCredentials(Guid UserId, string Email, string Role);

public interface IIdentityService
{
    Task<Result<Guid>> CreateUserAsync(string email, string password, string role, CancellationToken ct = default);
    Task<Result<UserCredentials>> ValidateCredentialsAsync(string email, string password, CancellationToken ct = default);
}
