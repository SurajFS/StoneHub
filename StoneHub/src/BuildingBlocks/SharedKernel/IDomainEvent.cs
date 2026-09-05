using MediatR;

namespace SharedKernel;

// Domain events are dispatched in-process via MediatR after the owning aggregate is
// persisted (see DomainEventDispatchInterceptor). Extending INotification lets modules
// react with INotificationHandler<TEvent> without any cross-module reference.
public interface IDomainEvent : INotification
{
    DateTimeOffset OccurredOn { get; }
}
