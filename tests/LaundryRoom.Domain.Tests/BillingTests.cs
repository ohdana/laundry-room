using Xunit;

public class BillingTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);

    private readonly Flat _flat;
    private readonly Flat _anotherFlat;
    private readonly Tariff _tariff;
    private readonly IList<JournalEntry> _entries;

    public BillingTests()
    {
        _flat = Flat.From("3B");
        _anotherFlat = Flat.From("2a");
        _tariff = Tariff.From(Money.From(27));
        _entries = new List<JournalEntry>();
    }

    [Fact]
    public void Billing_WhenNoEntries_ReturnsEmptyBill()
    {
        // Arrange
        // Act
        var bill = Billing.Calculate(_entries, _tariff);

        // Assert
        Assert.Empty(bill.Lines);
        Assert.Equal(Money.From(0), bill.Total);
    }

    [Fact]
    public void Billing_WhenFlatHasSingleEntry_ReturnsCost()
    {
        // Arrange
        _entries.Add(WasherEntry(_flat, 1.1m, Start));

        // Act
        var bill = Billing.Calculate(_entries, _tariff);

        // Assert
        var line = Assert.Single(bill.Lines);
        Assert.Equal(_flat, line.Flat);
        Assert.Equal(Money.From(30), line.Amount);
        Assert.Equal(Money.From(30), bill.Total);
    }

    [Fact]
    public void Billing_WhenFlatHasEntries_ReturnsTotalCost()
    {
        // Arrange
        _entries.Add(WasherEntry(_flat, 1.1m, Start));
        _entries.Add(DryerEntry(_flat, 1.5m, Start.Add(TimeSpan.FromMinutes(13))));
        _entries.Add(FanEntry(_flat, 2.7m, Start.Add(TimeSpan.FromMinutes(26))));

        // Act
        var bill = Billing.Calculate(_entries, _tariff);

        // Assert
        var line = Assert.Single(bill.Lines);
        Assert.Equal(Money.From(143), line.Amount);
        Assert.Equal(Money.From(143), bill.Total);
    }

    [Fact]
    public void Billing_WhenTwoFlatsHaveEntries_ReturnsLinePerFlat()
    {
        // Arrange
        _entries.Add(WasherEntry(_anotherFlat, 1.7m, Start.Subtract(TimeSpan.FromHours(2))));
        _entries.Add(WasherEntry(_flat, 1.1m, Start));
        _entries.Add(DryerEntry(_flat, 1.5m, Start.Add(TimeSpan.FromMinutes(13))));
        _entries.Add(FanEntry(_flat, 2.7m, Start.Add(TimeSpan.FromMinutes(26))));
        _entries.Add(FanEntry(_anotherFlat, 5.0m, Start.Add(TimeSpan.FromHours(2))));

        // Act
        var bill = Billing.Calculate(_entries, _tariff);

        // Assert
        Assert.Equal(2, bill.Lines.Count);
        var line = bill.Lines.Single(l => l.Flat == _flat);
        var anotherLine = bill.Lines.Single(l => l.Flat == _anotherFlat);
        Assert.Equal(Money.From(143), line.Amount);
        Assert.Equal(Money.From(181), anotherLine.Amount);
        Assert.Equal(Money.From(324), bill.Total);
    }

    private static JournalEntry WasherEntry(Flat flat, decimal kwh, DateTimeOffset startedAt)
    {
        var program = WasherProgram.Delicate;
        var outcome = new CycleOutcome.Completed(startedAt, startedAt.Add(program.Duration()));

        return new JournalEntry.Washer(Guid.NewGuid(), "green", flat, program, outcome, Kwh.From(kwh));
    }

    private static JournalEntry DryerEntry(Flat flat, decimal kwh, DateTimeOffset startedAt)
    {
        var program = DryerProgram.Normal;
        var outcome = new CycleOutcome.Completed(startedAt, startedAt.Add(program.Duration()));

        return new JournalEntry.Dryer(Guid.NewGuid(), "green", flat, program, outcome, Kwh.From(kwh));
    }
    
    private static JournalEntry FanEntry(Flat flat, decimal kwh, DateTimeOffset startedAt)
    {
        var program = FanProgram.Normal;
        var endedAt = startedAt.Add(TimeSpan.FromHours(4));

        return new JournalEntry.Fan(Guid.NewGuid(), "green", flat, program, startedAt, endedAt, Kwh.From(kwh));
    }
}