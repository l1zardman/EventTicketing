using EventTicketing.Domain.Common;
using EventTicketing.Domain.Events;

namespace EventTicketing.Application.Events;

// Read side. Intention-revealing methods only — no generic GetAll, no IQueryable
// crossing the boundary. Callers ask for what they mean; the query stays inside.
public interface IEventReadRepository
{
    Task<Event?> FindAsync(int eventId, CancellationToken ct = default);
    Task<IReadOnlyList<Event>> ListAsync(Specification<Event> specification, CancellationToken ct = default);
}
