using EventTicketing.Application.Abstractions.Messaging;
using EventTicketing.Domain.Common;
using EventTicketing.Domain.Events;

namespace EventTicketing.Application.Events;

public sealed record ReserveTicketsCommand(int EventId, int Quantity) : ICommand<Result<ReserveTicketsResult>>;

public sealed record ReserveTicketsResult(int EventId, int TicketsSold, int AvailableTickets);

public sealed class ReserveTicketsHandler
    : IRequestHandler<ReserveTicketsCommand, Result<ReserveTicketsResult>>
{
    private readonly IEventRepository _events;
    public ReserveTicketsHandler(IEventRepository events) => _events = events;

    public async Task<Result<ReserveTicketsResult>> HandleAsync(ReserveTicketsCommand command, CancellationToken ct = default)
    {
        var ev = await _events.GetByIdAsync(command.EventId, ct);
        if (ev is null)
            return EventErrors.NotFound(command.EventId);      // Error -> failure Result

        var reserve = ev.Reserve(command.Quantity);
        if (reserve.IsFailure)
            return reserve.Error;                              // propagate the domain failure

        return new ReserveTicketsResult(ev.Id, ev.TicketsSold, ev.AvailableTickets); // value -> success
    }
}
