namespace EventTicketing.Application.Events;

public sealed record EventAvailability(int EventId, string Name, int Capacity, int TicketsSold, int AvailableTickets);
