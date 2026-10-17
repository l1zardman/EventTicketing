using EventTicketing.Application.Abstractions;

namespace EventTicketing.Infrastructure.Persistence;

// The unit of work is the write DbContext's SaveChanges, behind the port the
// Application defined. Same scoped EventsDbContext the repository loaded from, so
// the tracked aggregate's changes are the ones that get committed.
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly EventsDbContext _db;
    public UnitOfWork(EventsDbContext db) => _db = db;

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
