using EventTicketing.Application.Abstractions;
using EventTicketing.Domain.Events;

namespace EventTicketing.Infrastructure.Payments;

// Stand-in for a real provider. It declines a sentinel email so the payment-failure
// path is demoable, and always accepts refunds. A real adapter would call Stripe/etc.
public sealed class FakePaymentGatewayAdapter : IPaymentGateway
{
    public Task<PaymentResult> ChargeAsync(string customerEmail, Money amount, CancellationToken ct = default)
    {
        if (customerEmail.Equals("decline@example.com", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(new PaymentResult(false, null));

        return Task.FromResult(new PaymentResult(true, $"CH-{Guid.NewGuid():N}"));
    }

    public Task<PaymentResult> RefundAsync(string paymentReference, Money amount, CancellationToken ct = default)
        => Task.FromResult(new PaymentResult(true, $"RF-{Guid.NewGuid():N}"));
}
