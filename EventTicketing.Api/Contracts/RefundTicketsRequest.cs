namespace EventTicketing.Api.Contracts;

public sealed record RefundTicketsRequest(int Quantity, string PaymentReference);
