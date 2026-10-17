namespace EventTicketing.Domain.Common;

public interface IDomainEvent { }

// Base for aggregate roots. Collects domain events raised while the aggregate
// changes; something on the outside (Module 9) will dispatch them after save.
public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
