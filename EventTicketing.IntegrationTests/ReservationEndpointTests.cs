using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EventTicketing.IntegrationTests;

// End-to-end: HTTP in, real database out, every layer and behavior exercised.
public class ReservationEndpointTests : IClassFixture<TicketingApiFactory>
{
    private readonly TicketingApiFactory _factory;
    public ReservationEndpointTests(TicketingApiFactory factory) => _factory = factory;

    private sealed record AvailabilityDto(int EventId, string Name, int Capacity, int TicketsSold, int AvailableTickets);

    [Fact]
    public async Task Reserving_available_tickets_succeeds_and_updates_availability()
    {
        var client = _factory.CreateClient();

        var reserve = await client.PostAsJsonAsync("/api/events/1/reservations", new { quantity = 2 });
        reserve.EnsureSuccessStatusCode();

        var availability = await client.GetFromJsonAsync<AvailabilityDto>("/api/events/1/availability");
        Assert.NotNull(availability);
        Assert.Equal(2, availability!.TicketsSold);
        Assert.Equal(98, availability.AvailableTickets);
    }

    [Fact]
    public async Task Oversell_returns_a_conflict_problem()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/events/1/reservations", new { quantity = 1_000_000 });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
