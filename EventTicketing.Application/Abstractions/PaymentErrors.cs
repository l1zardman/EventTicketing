using EventTicketing.Domain.Common;

namespace EventTicketing.Application.Abstractions;

public static class PaymentErrors
{
    public static Error Declined =>
        Error.Conflict("Payment.Declined", "The payment could not be processed.");

    public static Error RefundFailed =>
        Error.Conflict("Payment.RefundFailed", "The refund could not be processed.");
}
