public sealed record Kwh
{
    public decimal Value { get; }

    private Kwh(decimal value) => Value = value;

    public static Kwh From(decimal value) =>
        value < 0 ? throw new ArgumentException("KWh value can't be negative.", nameof(value))
                  : new Kwh(value);

    public static Kwh operator +(Kwh left, Kwh right) => From(left.Value + right.Value);
    public static Kwh operator -(Kwh left, Kwh right) => From(left.Value - right.Value);

    public static bool operator <(Kwh left, Kwh right)  => left.Value < right.Value;
    public static bool operator >(Kwh left, Kwh right)  => left.Value > right.Value;
    public static bool operator <=(Kwh left, Kwh right) => left.Value <= right.Value;
    public static bool operator >=(Kwh left, Kwh right) => left.Value >= right.Value;
}