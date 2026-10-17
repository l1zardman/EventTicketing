using EventTicketing.Domain.Common;
using Microsoft.Extensions.DependencyInjection;

namespace EventTicketing.Application.Abstractions.Messaging;

// Handles one kind of domain event. Many handlers may exist for the same event.
public interface IDomainEventHandler<in TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    Task HandleAsync(TDomainEvent domainEvent, CancellationToken ct = default);
}

// Fans each event out to every handler registered for its concrete type.
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken ct = default);
}

public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _provider;
    public DomainEventDispatcher(IServiceProvider provider) => _provider = provider;

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken ct = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            var method = handlerType.GetMethod("HandleAsync")!;

            foreach (var handler in _provider.GetServices(handlerType))
                await (Task)method.Invoke(handler, new object?[] { domainEvent, ct })!;
        }
    }
}
