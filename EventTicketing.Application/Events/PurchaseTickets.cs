using EventTicketing.Application.Abstractions;
using EventTicketing.Application.Abstractions.Messaging;
using EventTicketing.Domain.Common;
using EventTicketing.Domain.Events;
using FluentValidation;

namespace EventTicketing.Application.Events;

public sealed record PurchaseTicketsCommand(int EventId, int Quantity, string CustomerEmail)
    : ICommand<Result<PurchaseReceipt>>;

public sealed record PurchaseReceipt(int EventId, int Quantity, decimal Total, string Currency, string PaymentReference);

public sealed class PurchaseTicketsCommandValidator : AbstractValidator<PurchaseTicketsCommand>
{
    public PurchaseTicketsCommandValidator()
    {
        RuleFor(c => c.EventId).GreaterThan(0);
        RuleFor(c => c.Quantity).GreaterThan(0);
        RuleFor(c => c.CustomerEmail).NotEmpty().EmailAddress();
    }
}

public sealed class PurchaseTicketsHandler : IRequestHandler<PurchaseTicketsCommand, Result<PurchaseReceipt>>
{
    private readonly IEventRepository _events;
    private readonly IPaymentGateway _payments;

    public PurchaseTicketsHandler(IEventRepository events, IPaymentGateway payments)
    {
        _events = events;
        _payments = payments;
    }

    public async Task<Result<PurchaseReceipt>> HandleAsync(PurchaseTicketsCommand command, CancellationToken ct = default)
    {
        var ev = await _events.GetByIdAsync(command.EventId, ct);
        if (ev is null)
            return EventErrors.NotFound(command.EventId);

        // 1) Reserve capacity in memory. Sold out -> stop here, nothing charged.
        var reserve = ev.Reserve(command.Quantity);
        if (reserve.IsFailure)
            return reserve.Error;

        // 2) Take payment. A decline returns a failure BEFORE the pipeline commits,
        //    so the reservation is never persisted and the domain event never fires.
        var total = ev.PriceFor(command.Quantity); // pricing rule lives in the domain
        var payment = await _payments.ChargeAsync(command.CustomerEmail, total, ct);
        if (!payment.Succeeded)
            return PaymentErrors.Declined;

        // 3) Success: the UnitOfWorkBehavior commits, then TicketsReserved dispatches.
        return new PurchaseReceipt(ev.Id, command.Quantity, total.Amount, total.Currency, payment.Reference!);
    }
}
