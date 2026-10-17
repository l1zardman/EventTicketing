using EventTicketing.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace EventTicketing.Infrastructure.Persistence;

// Reads the rich Event aggregate. The mapping bends EF to the model, not the
// other way round: it materializes through private setters, converts the Money
// value object to the existing REAL "BasePrice" column, and ignores the members
// that aren't persistence — the computed availability and the domain events.
public sealed class EventsDbContext : DbContext
{
    public EventsDbContext(DbContextOptions<EventsDbContext> options) : base(options) { }

    public DbSet<Event> Events => Set<Event>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>(b =>
        {
            b.ToTable("Events");
            b.HasKey(e => e.Id);
            b.Property(e => e.Name);
            b.Property(e => e.Capacity);
            b.Property(e => e.TicketsSold);

            // Value object <-> existing column, via a converter.
            b.Property(e => e.BasePrice)
                .HasColumnName("BasePrice")
                .HasConversion(
                    money => (double)money.Amount,
                    value => Money.Of((decimal)value));

            b.Ignore(e => e.AvailableTickets); // computed, not stored
            b.Ignore(e => e.DomainEvents);     // behavior, not stored
            b.Property(e => e.Version).IsConcurrencyToken();
        });
    }
}
