using EventTicketing.Domain.Common;
using EventTicketing.Domain.Events;
using Xunit;

namespace EventTicketing.Domain.Tests;

public class MoneyTests
{
    [Fact]
    public void Of_rejects_a_negative_amount()
        => Assert.Throws<DomainException>(() => Money.Of(-1m));

    [Fact]
    public void Same_amount_and_currency_are_equal()
        => Assert.Equal(Money.Of(10m), Money.Of(10m));

    [Fact]
    public void Multiply_scales_the_amount()
        => Assert.Equal(Money.Of(30m), Money.Of(10m).Multiply(3));
}
