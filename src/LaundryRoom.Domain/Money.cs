public sealed record Money
{
    public long Value { get; }

    private Money(long value) => Value = value;

    public static Money From(long value) =>
        value < 0 ? throw new ArgumentException("Money value can't be negative.", nameof(value))
                  : new Money(value);

    public static Money operator +(Money left, Money right) => From(left.Value + right.Value);
    public static Money operator -(Money left, Money right) => From(left.Value - right.Value);

    public static bool operator <(Money left, Money right)  => left.Value < right.Value;
    public static bool operator >(Money left, Money right)  => left.Value > right.Value;
    public static bool operator <=(Money left, Money right) => left.Value <= right.Value;
    public static bool operator >=(Money left, Money right) => left.Value >= right.Value;
}