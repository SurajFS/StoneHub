using Billing.Application;
using Billing.Application.Dtos;
using Identity.Application;
using SharedKernel;

namespace Billing.Tests.Fakes;

// Billing's read side and Identity's user directory, scripted per test, for the entitlement and
// plans queries.
public sealed class FakeBillingDirectory : IBillingQueryService, IUserDirectory
{
    private readonly Dictionary<Guid, SubscriptionDto> _subscriptions = [];
    private readonly Dictionary<Guid, UserDirectoryEntry> _users = [];

    public Guid AddUser(string role, string? plan = null, string status = "Active", bool hasEverPaid = false)
    {
        var userId = Guid.NewGuid();
        _users[userId] = new UserDirectoryEntry(userId, role, role + " user", null);
        if (plan is not null)
        {
            var now = DateTimeOffset.UtcNow;
            _subscriptions[userId] = new SubscriptionDto(
                Guid.NewGuid(), userId, plan, status, now, null, null, now, WillRenew: false, hasEverPaid);
        }
        return userId;
    }

    public Task<SubscriptionDto?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        Task.FromResult(_subscriptions.GetValueOrDefault(userId));

    public Task<PagedResult<SubscriptionDto>> SearchAsync(int page, int pageSize, CancellationToken ct = default) =>
        throw new NotSupportedException("Not used by these tests.");

    public Task<UserDirectoryEntry?> GetAsync(Guid userId, CancellationToken ct = default) =>
        Task.FromResult(_users.GetValueOrDefault(userId));
}
