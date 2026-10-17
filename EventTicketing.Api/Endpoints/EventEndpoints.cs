using EventTicketing.Api.Contracts;
using EventTicketing.Api.Extensions;
using EventTicketing.Application.Abstractions.Messaging;
using EventTicketing.Application.Events;

namespace EventTicketing.Api.Endpoints;

public static class EventEndpoints
{
    public static IEndpointRouteBuilder MapEventEndpoints(this IEndpointRouteBuilder app)
    {
        var events = app.MapGroup("/api/events");

        events.MapGet("/available", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.SendAsync(new GetAvailableEventsQuery(), ct)));

        events.MapGet("/{eventId:int}/availability", async (int eventId, ISender sender, CancellationToken ct) =>
        {
            var availability = await sender.SendAsync(new GetEventAvailabilityQuery(eventId), ct);
            return availability is null ? Results.NotFound() : Results.Ok(availability);
        });

        events.MapPost("/{eventId:int}/reservations",
            async (int eventId, ReserveTicketsRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.SendAsync(new ReserveTicketsCommand(eventId, request.Quantity), ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
            });

        events.MapPost("/{eventId:int}/purchases",
            async (int eventId, PurchaseTicketsRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.SendAsync(
                    new PurchaseTicketsCommand(eventId, request.Quantity, request.CustomerEmail), ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
            });

        events.MapPost("/{eventId:int}/refunds",
            async (int eventId, RefundTicketsRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.SendAsync(
                    new RefundTicketsCommand(eventId, request.Quantity, request.PaymentReference), ct);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
            });

        return app;
    }
}
