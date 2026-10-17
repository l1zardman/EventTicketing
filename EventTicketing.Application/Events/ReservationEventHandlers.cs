using EventTicketing.Application.Abstractions.Messaging;
using EventTicketing.Domain.Events;
using Microsoft.Extensions.Logging;

namespace EventTicketing.Application.Events;

// Side effect #1: confirm the reservation. In a real app this would use an
// IEmailSender port; here it logs. It knows nothing about who reserved or why.
public sealed class SendReservationConfirmationHandler : IDomainEventHandler<TicketsReserved>
{
    private readonly ILogger<SendReservationConfirmationHandler> _logger;
    public SendReservationConfirmationHandler(ILogger<SendReservationConfirmationHandler> logger) => _logger = logger;

    public Task HandleAsync(TicketsReserved domainEvent, CancellationToken ct = default)
    {
        _logger.LogInformation("Reservation confirmed: {Quantity} ticket(s) for event {EventId}",
            domainEvent.Quantity, domainEvent.EventId);
        return Task.CompletedTask;
    }
}

// Side effect #2: record a metric. A second, completely independent handler for
// the same event — added without touching the first or the command handler.
public sealed class RecordReservationMetricHandler : IDomainEventHandler<TicketsReserved>
{
    private readonly ILogger<RecordReservationMetricHandler> _logger;
    public RecordReservationMetricHandler(ILogger<RecordReservationMetricHandler> logger) => _logger = logger;

    public Task HandleAsync(TicketsReserved domainEvent, CancellationToken ct = default)
    {
        _logger.LogInformation("Metric: tickets_reserved += {Quantity} (event {EventId})",
            domainEvent.Quantity, domainEvent.EventId);
        return Task.CompletedTask;
    }
}
