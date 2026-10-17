namespace EventTicketing.Api.Contracts;

public sealed record PurchaseTicketsRequest(int Quantity, string CustomerEmail);
