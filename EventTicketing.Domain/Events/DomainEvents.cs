using EventTicketing.Domain.Common;

namespace EventTicketing.Domain.Events;

public sealed record TicketsReserved(int EventId, int Quantity) : IDomainEvent;
public sealed record TicketsReleased(int EventId, int Quantity) : IDomainEvent;
