using Identity.Application.Dtos;

namespace Identity.Application;

// The single place that mints an access token + persists a rotated refresh token and packs
// them into an AuthResultDto. Used by login, register, and refresh.
public interface IAuthTokenIssuer
{
    Task<AuthResultDto> IssueAsync(Guid userId, string email, string role, CancellationToken ct = default);
}
