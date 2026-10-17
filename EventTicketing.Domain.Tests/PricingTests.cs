using EventTicketing.Domain.Events;
using Xunit;

namespace EventTicketing.Domain.Tests;

public class PricingTests
{
    private static Event EventAt(decimal unit)
        => Event.Create("Priced Event", 1000, Money.Of(unit));

    [Fact]
    public void Below_the_bulk_threshold_is_full_price()
    {
        // 8 * 50 = 400, no discount
        Assert.Equal(Money.Of(400m), EventAt(50m).PriceFor(8));
    }

    [Fact]
    public void At_the_bulk_threshold_gets_ten_percent_off()
    {
        // 10 * 50 = 500, minus 10% = 450
        Assert.Equal(Money.Of(450m), EventAt(50m).PriceFor(10));
    }
}
