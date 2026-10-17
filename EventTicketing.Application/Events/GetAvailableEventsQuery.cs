using EventTicketing.Application.Abstractions.Messaging;
using EventTicketing.Domain.Events;

namespace EventTicketing.Application.Events;

public sealed record EventSummary(int EventId, string Name, int AvailableTickets);

public sealed record GetAvailableEventsQuery() : IQuery<IReadOnlyList<EventSummary>>;

public sealed class GetAvailableEventsQueryHandler
    : IRequestHandler<GetAvailableEventsQuery, IReadOnlyList<EventSummary>>
{
    private readonly IEventReadRepository _events;
    public GetAvailableEventsQueryHandler(IEventReadRepository events) => _events = events;

    public async Task<IReadOnlyList<EventSummary>> HandleAsync(GetAvailableEventsQuery request, CancellationToken ct = default)
    {
        var events = await _events.ListAsync(new AvailableEventsSpecification(), ct);
        return events.Select(e => new EventSummary(e.Id, e.Name, e.AvailableTickets)).ToList();
    }
}
