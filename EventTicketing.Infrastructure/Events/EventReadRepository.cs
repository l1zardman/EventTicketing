using EventTicketing.Application.Events;
using EventTicketing.Domain.Common;
using EventTicketing.Domain.Events;
using EventTicketing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventTicketing.Infrastructure.Events;

public sealed class EventReadRepository : IEventReadRepository
{
    private readonly EventsDbContext _db;
    public EventReadRepository(EventsDbContext db) => _db = db;

    public Task<Event?> FindAsync(int eventId, CancellationToken ct = default)
        => _db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == eventId, ct);

    // The specification's expression is applied to the query here, inside
    // Infrastructure, and materialized. IQueryable never leaves this method.
    public async Task<IReadOnlyList<Event>> ListAsync(Specification<Event> specification, CancellationToken ct = default)
        => await _db.Events.AsNoTracking().Where(specification.ToExpression()).ToListAsync(ct);
}
