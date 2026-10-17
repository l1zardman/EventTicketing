using EventTicketing.Application.Abstractions.Messaging;
using EventTicketing.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EventTicketing.Infrastructure.Persistence;

// Runs AFTER the transaction commits: pulls the domain events off the tracked
// aggregates, clears them, and dispatches. Because it's in SavedChangesAsync,
// side effects only happen once the data is safely persisted.
public sealed class DomainEventDispatchInterceptor : SaveChangesInterceptor
{
    private readonly IDomainEventDispatcher _dispatcher;
    public DomainEventDispatchInterceptor(IDomainEventDispatcher dispatcher) => _dispatcher = dispatcher;

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken ct = default)
    {
        if (eventData.Context is not null)
            await DispatchAsync(eventData.Context, ct);

        return await base.SavedChangesAsync(eventData, result, ct);
    }

    private async Task DispatchAsync(DbContext context, CancellationToken ct)
    {
        var aggregates = context.ChangeTracker
            .Entries<AggregateRoot>()
            .Select(entry => entry.Entity)
            .Where(aggregate => aggregate.DomainEvents.Count > 0)
            .ToList();

        var domainEvents = aggregates.SelectMany(a => a.DomainEvents).ToList();

        foreach (var aggregate in aggregates)
            aggregate.ClearDomainEvents();

        await _dispatcher.DispatchAsync(domainEvents, ct);
    }
}
