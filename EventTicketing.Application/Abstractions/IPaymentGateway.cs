using EventTicketing.Domain.Events;

namespace EventTicketing.Application.Abstractions;

public interface IPaymentGateway
{
    Task<PaymentResult> ChargeAsync(string customerEmail, Money amount, CancellationToken ct = default);
    Task<PaymentResult> RefundAsync(string paymentReference, Money amount, CancellationToken ct = default);
}

public sealed record PaymentResult(bool Succeeded, string? Reference);
