using MediatR;
using SharedKernel;

namespace Notifications.Application.Devices;

public sealed record RegisterDeviceCommand(Guid UserId, string ExpoPushToken, string Platform) : IRequest<Result>;
