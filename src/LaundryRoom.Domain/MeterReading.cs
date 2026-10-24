public sealed record MeterReading
{
    public decimal Value { get; }

    private MeterReading(decimal value) => Value = value;

    public static MeterReading From(decimal value) =>
        value < 0 ? throw new ArgumentException("Meter value can't be negative.", nameof(value))
                  : new MeterReading(value);
}