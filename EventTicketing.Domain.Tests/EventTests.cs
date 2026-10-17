using EventTicketing.Domain.Common;
using EventTicketing.Domain.Events;
using Xunit;

namespace EventTicketing.Domain.Tests;

// Pure, fast, no mocks. The aggregate depends on nothing, so neither do its tests.
public class EventTests
{
    private static Event NewEvent(int capacity = 100, int reserve = 0)
    {
        var ev = Event.Create("Test Event", capacity, Money.Of(49.99m));
        if (reserve > 0) ev.Reserve(reserve);
        return ev;
    }

    [Fact]
    public void Create_rejects_a_blank_name()
        => Assert.Throws<DomainException>(() => Event.Create("  ", 100, Money.Of(10m)));

    [Fact]
    public void Create_rejects_nonpositive_capacity()
        => Assert.Throws<DomainException>(() => Event.Create("X", 0, Money.Of(10m)));

    [Fact]
    public void Reserve_reduces_availability()
    {
        var ev = NewEvent(capacity: 100);
        var result = ev.Reserve(2);
        Assert.True(result.IsSuccess);
        Assert.Equal(2, ev.TicketsSold);
        Assert.Equal(98, ev.AvailableTickets);
    }

    [Fact]
    public void Reserving_beyond_capacity_returns_a_conflict_failure()
    {
        var ev = NewEvent(capacity: 1);
        var result = ev.Reserve(2);
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(0, ev.TicketsSold);
    }

    [Fact]
    public void Release_returns_capacity()
    {
        var ev = NewEvent(capacity: 100, reserve: 5);
        var result = ev.Release(2);
        Assert.True(result.IsSuccess);
        Assert.Equal(3, ev.TicketsSold);
        Assert.Equal(97, ev.AvailableTickets);
    }

    [Fact]
    public void Releasing_more_than_sold_returns_a_conflict_failure()
    {
        var ev = NewEvent(capacity: 100, reserve: 1);
        var result = ev.Release(2);
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public void Reserve_raises_a_domain_event()
    {
        var ev = NewEvent();
        ev.Reserve(3);
        var reserved = Assert.IsType<TicketsReserved>(Assert.Single(ev.DomainEvents));
        Assert.Equal(3, reserved.Quantity);
    }
}
