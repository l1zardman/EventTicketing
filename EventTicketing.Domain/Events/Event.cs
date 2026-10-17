using EventTicketing.Domain.Common;

namespace EventTicketing.Domain.Events;

public sealed class Event : AggregateRoot
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int Capacity { get; private set; }
    public int TicketsSold { get; private set; }
    public Money BasePrice { get; private set; } = null!;
    public int Version { get; private set; }

    public int AvailableTickets => Capacity - TicketsSold;

    private const int BulkThreshold = 10;
    private const decimal BulkDiscountRate = 0.10m;

    // Pricing is a business rule: the bulk discount would be true no matter what
    // framework, database, or UI sat around it. So it lives in the domain.
    public Money PriceFor(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be positive.");

        var subtotal = BasePrice.Multiply(quantity);
        return quantity >= BulkThreshold ? subtotal.ApplyDiscount(BulkDiscountRate) : subtotal;
    }

    private Event() { }

    private Event(string name, int capacity, Money basePrice)
    {
        Name = name;
        Capacity = capacity;
        BasePrice = basePrice;
        TicketsSold = 0;
    }

    public static Event Create(string name, int capacity, Money basePrice)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Event name is required.");
        if (capacity <= 0)
            throw new DomainException("Capacity must be positive.");
        ArgumentNullException.ThrowIfNull(basePrice);

        return new Event(name, capacity, basePrice);
    }

    // "Sold out" is an EXPECTED outcome, so it's a returned failure, not a throw.
    // A non-positive quantity is a caller bug (validation catches it first), so it
    // stays a thrown DomainException — the two categories, side by side.
    public Result Reserve(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Reservation quantity must be positive.");

        if (TicketsSold + quantity > Capacity)
            return Result.Failure(EventErrors.InsufficientCapacity(AvailableTickets, quantity));

        TicketsSold += quantity;
        Version++;
        RaiseDomainEvent(new TicketsReserved(Id, quantity));
        return Result.Success();
    }

    // Like Reserve, an over-release is now an EXPECTED failure (a user double-refunding),
    // returned as a Result. A non-positive quantity is still a caller bug -> throw.
    public Result Release(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Release quantity must be positive.");

        if (quantity > TicketsSold)
            return Result.Failure(EventErrors.TooManyToRelease(TicketsSold, quantity));

        TicketsSold -= quantity;
        Version++;
        RaiseDomainEvent(new TicketsReleased(Id, quantity));
        return Result.Success();
    }
}
