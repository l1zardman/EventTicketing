using EventTicketing.Application.Abstractions;
using EventTicketing.Application.Events;
using EventTicketing.Domain.Common;
using EventTicketing.Domain.Events;
using Xunit;

namespace EventTicketing.Application.Tests;

public class PurchaseTicketsHandlerTests
{
    private sealed class FakeEventRepository : IEventRepository
    {
        private readonly Dictionary<int, Event> _byId;
        public FakeEventRepository(params (int Id, Event Event)[] events)
            => _byId = events.ToDictionary(e => e.Id, e => e.Event);
        public Task<Event?> GetByIdAsync(int eventId, CancellationToken ct = default)
            => Task.FromResult(_byId.GetValueOrDefault(eventId));
    }

    private sealed class StubPaymentGateway : IPaymentGateway
    {
        private readonly bool _succeeds;
        public StubPaymentGateway(bool succeeds) => _succeeds = succeeds;
        public Task<PaymentResult> ChargeAsync(string customerEmail, Money amount, CancellationToken ct = default)
            => Task.FromResult(new PaymentResult(_succeeds, _succeeds ? "CH-TEST" : null));
        public Task<PaymentResult> RefundAsync(string paymentReference, Money amount, CancellationToken ct = default)
            => Task.FromResult(new PaymentResult(true, "RF-TEST"));
    }

    private static Event EventWithCapacity(int capacity)
        => Event.Create("Test Event", capacity, Money.Of(49.99m));

    [Fact]
    public async Task Successful_purchase_prices_and_charges()
    {
        var handler = new PurchaseTicketsHandler(
            new FakeEventRepository((1, EventWithCapacity(100))),
            new StubPaymentGateway(succeeds: true));

        var result = await handler.HandleAsync(new PurchaseTicketsCommand(1, 2, "reader@example.com"));

        Assert.True(result.IsSuccess);
        Assert.Equal(99.98m, result.Value.Total);
        Assert.Equal("CH-TEST", result.Value.PaymentReference);
    }

    [Fact]
    public async Task Bulk_purchase_gets_the_discount()
    {
        var handler = new PurchaseTicketsHandler(
            new FakeEventRepository((1, Event.Create("Test", 100, Money.Of(50m)))),
            new StubPaymentGateway(succeeds: true));

        var result = await handler.HandleAsync(new PurchaseTicketsCommand(1, 10, "reader@example.com"));

        Assert.True(result.IsSuccess);
        Assert.Equal(450m, result.Value.Total); // 10 * 50 = 500, minus 10%
    }

    [Fact]
    public async Task Declined_payment_is_a_conflict_failure()
    {
        var handler = new PurchaseTicketsHandler(
            new FakeEventRepository((1, EventWithCapacity(100))),
            new StubPaymentGateway(succeeds: false));

        var result = await handler.HandleAsync(new PurchaseTicketsCommand(1, 2, "reader@example.com"));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }
}
