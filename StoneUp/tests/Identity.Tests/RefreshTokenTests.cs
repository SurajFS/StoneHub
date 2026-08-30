using Identity.Domain;
using Xunit;

namespace Identity.Tests;

public sealed class RefreshTokenTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Issue_IsActive_BeforeExpiryAndNotRevoked()
    {
        var token = RefreshToken.Issue(Guid.NewGuid(), "hash", Now.AddDays(30));

        Assert.True(token.IsActive(Now));
        Assert.Null(token.RevokedAt);
    }

    [Fact]
    public void Expired_IsNotActive()
    {
        var token = RefreshToken.Issue(Guid.NewGuid(), "hash", Now.AddDays(-1));

        Assert.False(token.IsActive(Now));
    }

    [Fact]
    public void Revoked_IsNotActive()
    {
        var token = RefreshToken.Issue(Guid.NewGuid(), "hash", Now.AddDays(30));

        token.Revoke(Now);

        Assert.False(token.IsActive(Now));
    }

    [Fact]
    public void Revoke_IsIdempotent_KeepsFirstTimestamp()
    {
        var token = RefreshToken.Issue(Guid.NewGuid(), "hash", Now.AddDays(30));

        token.Revoke(Now);
        token.Revoke(Now.AddDays(1));

        Assert.Equal(Now, token.RevokedAt);
    }
}
