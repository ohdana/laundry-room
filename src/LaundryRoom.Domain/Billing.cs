public static class Billing
{
    public static Money Calculate(Flat flat, IEnumerable<JournalEntry> entries, Tariff tariff)
    {
        var total = entries
            .Where(entry => entry.CardOwner == flat)
            .Aggregate(Kwh.From(0), (sum, entry) => sum + entry.ElectricityConsumed);
        
        return tariff.Cost(total);
    }
}