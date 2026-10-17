using EventTicketing.Api;
using EventTicketing.Api.Endpoints;
using EventTicketing.Api.Features.Schedule;
using EventTicketing.Application;
using EventTicketing.Domain.Events;
using EventTicketing.Infrastructure;
using EventTicketing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Default")!;

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);

var app = builder.Build();

// The clean persistence now owns schema creation and seeding — the legacy
// AppDbContext that used to do this is gone.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EventsDbContext>();
    await db.Database.EnsureCreatedAsync();

    if (!await db.Events.AnyAsync())
    {
        db.Events.Add(Event.Create("Clean Architecture Live", 100, Money.Of(49.99m)));
        await db.SaveChangesAsync();
    }
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Everything is a minimal-API endpoint now — no controllers left.
app.MapEventEndpoints();
app.MapGetEventSchedule();

app.Run();

public partial class Program { }
