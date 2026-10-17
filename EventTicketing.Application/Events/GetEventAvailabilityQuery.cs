using EventTicketing.Application.Abstractions.Messaging;

namespace EventTicketing.Application.Events;

public sealed record GetEventAvailabilityQuery(int EventId) : IQuery<EventAvailability?>;

public sealed class GetEventAvailabilityQueryHandler
    : IRequestHandler<GetEventAvailabilityQuery, EventAvailability?>
{
    private readonly IEventReadRepository _events;
    public GetEventAvailabilityQueryHandler(IEventReadRepository events) => _events = events;

    public async Task<EventAvailability?> HandleAsync(GetEventAvailabilityQuery request, CancellationToken ct = default)
    {
        var ev = await _events.FindAsync(request.EventId, ct);
        return ev is null
            ? null
            : new EventAvailability(ev.Id, ev.Name, ev.Capacity, ev.TicketsSold, ev.AvailableTickets);
    }
}
