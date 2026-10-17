using EventTicketing.Domain.Common;

namespace EventTicketing.Domain.Events;

// A value object: immutable, compared by value (record), and impossible to
// construct in an invalid state. No identity — two Money values with the same
// amount and currency ARE the same money.
public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Of(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            throw new DomainException("Money cannot be negative.");
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Currency is required.");

        return new Money(amount, currency);
    }

    public static Money Zero(string currency = "USD") => new(0m, currency);

    public Money Multiply(int quantity) => new(Amount * quantity, Currency);

    public Money ApplyDiscount(decimal rate)
    {
        if (rate is < 0m or > 1m)
            throw new DomainException("Discount rate must be between 0 and 1.");

        return new Money(Amount * (1m - rate), Currency);
    }

    public override string ToString() => $"{Amount:0.00} {Currency}";
}
