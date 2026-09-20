namespace Notifications.Domain;

public interface IDeviceTokenRepository
{
    Task<DeviceToken?> GetByTokenAsync(string expoPushToken, CancellationToken ct = default);

    Task<IReadOnlyList<DeviceToken>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    void Add(DeviceToken token);

    Task SaveChangesAsync(CancellationToken ct = default);
}
