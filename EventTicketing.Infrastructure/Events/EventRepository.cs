using EventTicketing.Application.Events;
using EventTicketing.Domain.Events;
using EventTicketing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventTicketing.Infrastructure.Events;

// Loads a *tracked* aggregate so the changes it makes to itself are detected.
// It no longer saves — committing is the unit of work's job now.
public sealed class EventRepository : IEventRepository
{
    private readonly EventsDbContext _db;
    public EventRepository(EventsDbContext db) => _db = db;

    public Task<Event?> GetByIdAsync(int eventId, CancellationToken ct = default)
        => _db.Events.FirstOrDefaultAsync(e => e.Id == eventId, ct);
}
