using EventTicketing.Application.Events;
using EventTicketing.Domain.Common;
using EventTicketing.Domain.Events;
using Xunit;

namespace EventTicketing.Application.Tests;

// The handler depends on a port (IEventRepository), so a tiny hand-written fake
// replaces it. No database, no mediator, no web host — just the use case.
public class ReserveTicketsHandlerTests
{
    private sealed class FakeEventRepository : IEventRepository
    {
        private readonly Dictionary<int, Event> _byId;
        public FakeEventRepository(params (int Id, Event Event)[] events)
            => _byId = events.ToDictionary(e => e.Id, e => e.Event);

        public Task<Event?> GetByIdAsync(int eventId, CancellationToken ct = default)
            => Task.FromResult(_byId.GetValueOrDefault(eventId));
    }

    private static Event EventWithCapacity(int capacity)
        => Event.Create("Test Event", capacity, Money.Of(49.99m));

    [Fact]
    public async Task Reserves_and_reports_new_availability()
    {
        var handler = new ReserveTicketsHandler(new FakeEventRepository((1, EventWithCapacity(100))));

        var result = await handler.HandleAsync(new ReserveTicketsCommand(1, 3));

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.TicketsSold);
        Assert.Equal(97, result.Value.AvailableTickets);
    }

    [Fact]
    public async Task Missing_event_is_a_not_found_failure()
    {
        var handler = new ReserveTicketsHandler(new FakeEventRepository()); // empty

        var result = await handler.HandleAsync(new ReserveTicketsCommand(999, 1));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Oversell_is_a_conflict_failure()
    {
        var handler = new ReserveTicketsHandler(new FakeEventRepository((1, EventWithCapacity(1))));

        var result = await handler.HandleAsync(new ReserveTicketsCommand(1, 5));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }
}
