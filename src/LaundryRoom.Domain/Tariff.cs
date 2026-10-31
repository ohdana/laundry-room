public sealed record Tariff
{
    public Money PricePerKwh { get; }
    
    private Tariff(Money pricePerKwh) => PricePerKwh = pricePerKwh;

    public static Tariff From(Money pricePerKwh) => new Tariff(pricePerKwh);

    public Money Cost(Kwh energy)
    {
        var total = Math.Round(energy.Value * PricePerKwh.Value,
            MidpointRounding.AwayFromZero);
        return Money.From((long) total);
    }
}