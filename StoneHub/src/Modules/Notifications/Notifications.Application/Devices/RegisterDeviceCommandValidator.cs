using FluentValidation;
using Notifications.Domain;

namespace Notifications.Application.Devices;

public sealed class RegisterDeviceCommandValidator : AbstractValidator<RegisterDeviceCommand>
{
    public RegisterDeviceCommandValidator()
    {
        RuleFor(x => x.ExpoPushToken).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Platform)
            .Must(p => Enum.TryParse<DevicePlatform>(p, ignoreCase: true, out _))
            .WithMessage("Platform must be 'Ios' or 'Android'.");
    }
}
