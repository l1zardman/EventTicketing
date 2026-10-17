using EventTicketing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventTicketing.Api.Features.Schedule;

// A VERTICAL SLICE. Everything this feature needs lives here: the endpoint, its
// response shape, and its data access — talking straight to EF. No mediator, no
// command/handler, no repository port, no domain mapping. For a simple read, this
// is dramatically less code and indirection than the layered path. It also, quite
// deliberately, couples this endpoint to Entity Framework — the trade VSA makes.
public static class GetEventSchedule
{
    public sealed record ScheduleItem(int EventId, string Name, int Capacity, int TicketsSold);

    public static IEndpointRouteBuilder MapGetEventSchedule(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/schedule", async (EventsDbContext db, CancellationToken ct) =>
        {
            var schedule = await db.Events
                .AsNoTracking()
                .OrderBy(e => e.Name)
                .Select(e => new ScheduleItem(e.Id, e.Name, e.Capacity, e.TicketsSold))
                .ToListAsync(ct);

            return Results.Ok(schedule);
        });

        return app;
    }
}
