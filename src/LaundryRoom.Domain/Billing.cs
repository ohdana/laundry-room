public sealed record BillLine(Flat Flat, Money Amount);
public sealed record Bill(IReadOnlyList<BillLine> Lines, Money Total);

public static class Billing
{
    public static Money Calculate(Flat flat, IEnumerable<JournalEntry> entries, Tariff tariff)
    {
        var total = entries
            .Where(entry => entry.CardOwner == flat)
            .Aggregate(Kwh.From(0), (sum, entry) => sum + entry.ElectricityConsumed);
        
        return tariff.Cost(total);
    }

    public static Bill Calculate(IEnumerable<JournalEntry> entries, Tariff tariff)
    {
        var flats = entries.Select(entry => entry.CardOwner).Distinct().ToList();
        var energies = flats.Select(flat => EnergyOf(flat, entries)).ToList();

        var total = tariff.Cost(Sum(energies));
        if (flats.Count == 0) return new Bill(new List<BillLine>(), total);

        var shares = Allocator.Allocate(total, energies);
        var lines = flats.Select((flat, i) => new BillLine(flat, shares[i])).ToList();

        return new Bill(lines, total);
    }

    private static Kwh EnergyOf(Flat flat, IEnumerable<JournalEntry> entries)
    {
        return entries
            .Where(entry => entry.CardOwner == flat)
            .Aggregate(Kwh.From(0), (sum, entry) => sum + entry.ElectricityConsumed);
    }

    private static Kwh Sum(IEnumerable<Kwh> energies)
    {
        return energies.Aggregate(Kwh.From(0), (sum, energy) => sum + energy);
    }
}