namespace SharedKernel;

// Non-generic view over an aggregate's domain events so infrastructure (the EF
// SaveChanges interceptor) can collect and clear them without knowing the aggregate's
// TId. AggregateRoot<TId> implements this.
public interface IHasDomainEvents
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}
