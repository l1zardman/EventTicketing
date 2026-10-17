namespace EventTicketing.Api.Contracts;

// An edge model: the shape of the request BODY. It differs from the command
// (which also carries the eventId from the route), so this separation pays for
// itself. Responses, by contrast, reuse the Application DTOs directly.
public sealed record ReserveTicketsRequest(int Quantity);
