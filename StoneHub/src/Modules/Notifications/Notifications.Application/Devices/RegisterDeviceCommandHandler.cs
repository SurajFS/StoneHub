using MediatR;
using Notifications.Domain;
using SharedKernel;

namespace Notifications.Application.Devices;

public sealed class RegisterDeviceCommandHandler(IDeviceTokenRepository tokens)
    : IRequestHandler<RegisterDeviceCommand, Result>
{
    public async Task<Result> Handle(RegisterDeviceCommand request, CancellationToken ct)
    {
        var platform = Enum.Parse<DevicePlatform>(request.Platform, ignoreCase: true);

        var existing = await tokens.GetByTokenAsync(request.ExpoPushToken, ct);
        if (existing is null)
            tokens.Add(DeviceToken.Create(request.UserId, request.ExpoPushToken, platform));
        else
            existing.ReassignTo(request.UserId, platform);

        await tokens.SaveChangesAsync(ct);
        return Result.Success();
    }
}
