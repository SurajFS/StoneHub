namespace SharedKernel;

public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}
