using EventTicketing.Application.Abstractions;
using EventTicketing.Application.Abstractions.Messaging;
using EventTicketing.Domain.Common;
using EventTicketing.Domain.Events;
using FluentValidation;

namespace EventTicketing.Application.Events;

public sealed record RefundTicketsCommand(int EventId, int Quantity, string PaymentReference)
    : ICommand<Result<RefundReceipt>>;

public sealed record RefundReceipt(int EventId, int Quantity, decimal Refunded, string Currency, string RefundReference);

public sealed class RefundTicketsCommandValidator : AbstractValidator<RefundTicketsCommand>
{
    public RefundTicketsCommandValidator()
    {
        RuleFor(c => c.EventId).GreaterThan(0);
        RuleFor(c => c.Quantity).GreaterThan(0);
        RuleFor(c => c.PaymentReference).NotEmpty();
    }
}

public sealed class RefundTicketsHandler : IRequestHandler<RefundTicketsCommand, Result<RefundReceipt>>
{
    private readonly IEventRepository _events;
    private readonly IPaymentGateway _payments;

    public RefundTicketsHandler(IEventRepository events, IPaymentGateway payments)
    {
        _events = events;
        _payments = payments;
    }

    public async Task<Result<RefundReceipt>> HandleAsync(RefundTicketsCommand command, CancellationToken ct = default)
    {
        var ev = await _events.GetByIdAsync(command.EventId, ct);
        if (ev is null)
            return EventErrors.NotFound(command.EventId);

        // 1) Return the capacity. Releasing more than sold -> expected failure.
        var release = ev.Release(command.Quantity);
        if (release.IsFailure)
            return release.Error;

        // 2) Refund the money.
        var amount = ev.BasePrice.Multiply(command.Quantity);
        var refund = await _payments.RefundAsync(command.PaymentReference, amount, ct);
        if (!refund.Succeeded)
            return PaymentErrors.RefundFailed;

        return new RefundReceipt(ev.Id, command.Quantity, amount.Amount, amount.Currency, refund.Reference!);
    }
}
