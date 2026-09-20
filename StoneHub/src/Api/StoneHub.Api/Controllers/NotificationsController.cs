using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notifications.Application.Devices;

namespace StoneHub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/notifications")]
public sealed class NotificationsController(IMediator mediator) : ControllerBase
{
    // Called on login/app-foreground with the device's Expo push token. Re-registering an
    // existing token is a normal no-op update, not an error.
    [HttpPost("devices")]
    public async Task<IActionResult> RegisterDevice(RegisterDeviceRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new RegisterDeviceCommand(CallerUserId, request.ExpoPushToken, request.Platform), ct);
        return result.ToActionResult();
    }

    private Guid CallerUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

public sealed record RegisterDeviceRequest(string ExpoPushToken, string Platform);
