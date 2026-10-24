public sealed record MeterReading
{
    public Kwh Total { get; }
    private MeterReading(Kwh total) => Total = total;
    public static MeterReading From(Kwh total) =>
        new MeterReading(total ?? throw new ArgumentNullException(nameof(total)));
}