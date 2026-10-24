public sealed record MeterReading
{
    public Kwh Kwh { get; }
    private MeterReading(Kwh kwh) => Kwh = kwh;
    public static MeterReading From(Kwh kwh) => new MeterReading(kwh);
}