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
    public void Billing_WhenFlatHasNoEntries_ReturnsZero()
    {
        // Arrange
        // Act
        var total = Billing.Calculate(_flat, _entries, _tariff);

        // Assert
        Assert.Equal(0, total.Value);
    }

    [Fact]
    public void Billing_WhenFlatHasSingleEntry_ReturnsCost()
    {
        // Arrange
        _entries.Add(WasherEntry(_flat, 1.1m, Start));

        // Act
        var total = Billing.Calculate(_flat, _entries, _tariff);

        // Assert
        Assert.Equal(Money.From(30), total);
    }

    [Fact]
    public void Billing_WhenFlatHasEntries_ReturnsTotalCost()
    {
        // Arrange
        var washerKwh = 1.1m;
        var dryerKwh = 1.5m;
        var fanKwh = 2.7m;

        _entries.Add(WasherEntry(_flat, washerKwh, Start));
        _entries.Add(DryerEntry(_flat, dryerKwh, Start.Add(TimeSpan.FromMinutes(13))));
        _entries.Add(FanEntry(_flat, fanKwh, Start.Add(TimeSpan.FromMinutes(26))));

        var expectedTotal = _tariff.Cost(Kwh.From(washerKwh + dryerKwh + fanKwh));

        // Act
        var actualTotal = Billing.Calculate(_flat, _entries, _tariff);

        // Assert
        Assert.Equal(Money.From(143), actualTotal);
    }

    [Fact]
    public void Billing_WhenFlatHasEntries_IgnoresOtherFlatsEntries()
    {
        // Arrange
        var washerKwh = 1.1m;
        var dryerKwh = 1.5m;
        var fanKwh = 2.7m;

        var anotherWasherKwh = 1.7m;
        var anotherFanKwh = 5.0m;

        _entries.Add(WasherEntry(_anotherFlat, anotherWasherKwh, Start.Subtract(TimeSpan.FromHours(2))));
        _entries.Add(WasherEntry(_flat, washerKwh, Start));
        _entries.Add(DryerEntry(_flat, dryerKwh, Start.Add(TimeSpan.FromMinutes(13))));
        _entries.Add(FanEntry(_flat, fanKwh, Start.Add(TimeSpan.FromMinutes(26))));
        _entries.Add(FanEntry(_anotherFlat, anotherFanKwh, Start.Add(TimeSpan.FromHours(2))));

        // Act
        var actualTotal = Billing.Calculate(_flat, _entries, _tariff);

        // Assert
        Assert.Equal(Money.From(143), actualTotal);
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