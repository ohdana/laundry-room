using Xunit;
using NSubstitute;
using Microsoft.Extensions.Time.Testing;

public class JournalIntegrationTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);

    private readonly FakeTimeProvider _clock = new(Start);
    private readonly IPowerSource _powerSource = Substitute.For<IPowerSource>();
    private readonly IElectricityMeter _meter = Substitute.For<IElectricityMeter>();
    private readonly Guid _washerId = Guid.NewGuid();
    private readonly Guid _dryerId = Guid.NewGuid();
    private readonly Journal _journal;
    
    public JournalIntegrationTests()
    {
        _meter.Read().Returns(Reading(0));
        _journal = new Journal(_clock);
    }

    [Theory]
    [MemberData(nameof(GetWasherPrograms))]
    public void Journal_WhenAttachedToWasherAndWasherStarted_YieldsNoEntry(WasherProgram program)
    {
        // Arrange
        var washer = GetAttachedPoweredWasher();
        _clock.Advance(TimeSpan.FromMinutes(5));

        // Act
        washer.StartProgram(program);
        _clock.Advance(program.Duration() / 2);

        // Assert
        Assert.Empty(_journal.Entries);
    }

    [Theory]
    [MemberData(nameof(GetWasherPrograms))]
    public void Journal_WhenAttachedToWasherAndWasherCompleted_YieldsCompletedEntry(WasherProgram program)
    {
        // Arrange
        var washer = GetAttachedPoweredWasher();
        _clock.Advance(TimeSpan.FromMinutes(5));
        washer.StartProgram(program);

        // Act
        var startedAt = _clock.GetUtcNow();
        _clock.Advance(program.Duration());

        // Assert
        var entry = Assert.IsType<JournalEntry.Washer>(Assert.Single(_journal.Entries));
        Assert.Equal(_washerId, entry.WasherId);
        Assert.Equal(program, entry.Program);
        
        var outcome = Assert.IsType<CycleOutcome.Completed>(entry.Outcome);
        Assert.Equal(startedAt, outcome.StartedAt);
        Assert.Equal(startedAt.Add(program.Duration()), outcome.CompletedAt);
    }

    [Theory]
    [MemberData(nameof(GetWasherPrograms))]
    public void Journal_WhenAttachedToWasherAndWasherInterrupted_YieldsInterruptedEntry(WasherProgram program)
    {
        // Arrange
        var washer = GetAttachedPoweredWasher();
        _clock.Advance(TimeSpan.FromMinutes(5));
        washer.StartProgram(program);

        // Act
        var startedAt = _clock.GetUtcNow();
        var elapsed = program.Duration() / 2;
        _clock.Advance(elapsed);
        washer.SwitchOff();

        // Assert
        var entry = Assert.IsType<JournalEntry.Washer>(Assert.Single(_journal.Entries));
        Assert.Equal(_washerId, entry.WasherId);
        Assert.Equal(program, entry.Program);
        
        var outcome = Assert.IsType<CycleOutcome.Interrupted>(entry.Outcome);
        Assert.Equal(startedAt, outcome.StartedAt);
        Assert.Equal(startedAt.Add(elapsed), outcome.InterruptedAt);
    }

    [Theory]
    [MemberData(nameof(GetWasherProgramsWithTwoOutcomes))]
    public void Journal_WhenTwoWasherCyclesEnded_YieldsTwoEntriesInOrder(
        WasherProgram program, Type outcome1, Type outcome2)
    {
        // Arrange
        var washer = GetAttachedPoweredWasher();
        _clock.Advance(TimeSpan.FromMinutes(5));

        // Act
        RunWasherCycleWithOutcome(washer, program, outcome1);
        RunWasherCycleWithOutcome(washer, program, outcome2);

        // Assert
        Assert.Equal(2, _journal.Entries.Count);
        Assert.IsType(outcome1, Assert.IsType<JournalEntry.Washer>(_journal.Entries[0]).Outcome);
        Assert.IsType(outcome2, Assert.IsType<JournalEntry.Washer>(_journal.Entries[1]).Outcome);
    }



    [Theory]
    [MemberData(nameof(GetDryerPrograms))]
    public void Journal_WhenAttachedToDryerAndDryerStarted_YieldsNoEntry(DryerProgram program)
    {
        // Arrange
        var dryer = GetAttachedPoweredDryer();
        _clock.Advance(TimeSpan.FromMinutes(5));

        // Act
        dryer.StartProgram(program);
        _clock.Advance(program.Duration() / 2);

        // Assert
        Assert.Empty(_journal.Entries);
    }

    [Theory]
    [MemberData(nameof(GetDryerPrograms))]
    public void Journal_WhenAttachedToDryerAndDryerCompleted_YieldsCompletedEntry(DryerProgram program)
    {
        // Arrange
        var dryer = GetAttachedPoweredDryer();
        _clock.Advance(TimeSpan.FromMinutes(5));
        dryer.StartProgram(program);

        // Act
        var startedAt = _clock.GetUtcNow();
        _clock.Advance(program.Duration());

        // Assert
        var entry = Assert.IsType<JournalEntry.Dryer>(Assert.Single(_journal.Entries));
        Assert.Equal(_dryerId, entry.DryerId);
        Assert.Equal(program, entry.Program);
        
        var outcome = Assert.IsType<CycleOutcome.Completed>(entry.Outcome);
        Assert.Equal(startedAt, outcome.StartedAt);
        Assert.Equal(startedAt.Add(program.Duration()), outcome.CompletedAt);
    }

    [Theory]
    [MemberData(nameof(GetDryerPrograms))]
    public void Journal_WhenAttachedToDryerAndDryerInterrupted_YieldsInterruptedEntry(DryerProgram program)
    {
        // Arrange
        var dryer = GetAttachedPoweredDryer();
        _clock.Advance(TimeSpan.FromMinutes(5));
        dryer.StartProgram(program);

        // Act
        var startedAt = _clock.GetUtcNow();
        var elapsed = program.Duration() / 2;
        _clock.Advance(elapsed);
        dryer.SwitchOff();

        // Assert
        var entry = Assert.IsType<JournalEntry.Dryer>(Assert.Single(_journal.Entries));
        Assert.Equal(_dryerId, entry.DryerId);
        Assert.Equal(program, entry.Program);
        
        var outcome = Assert.IsType<CycleOutcome.Interrupted>(entry.Outcome);
        Assert.Equal(startedAt, outcome.StartedAt);
        Assert.Equal(startedAt.Add(elapsed), outcome.InterruptedAt);
    }

    [Theory]
    [MemberData(nameof(GetDryerProgramsWithTwoOutcomes))]
    public void Journal_WhenTwoDryerCyclesEnded_YieldsTwoEntriesInOrder(
        DryerProgram program, Type outcome1, Type outcome2)
    {
        // Arrange
        var dryer = GetAttachedPoweredDryer();
        _clock.Advance(TimeSpan.FromMinutes(5));

        // Act
        RunDryerCycleWithOutcome(dryer, program, outcome1);
        RunDryerCycleWithOutcome(dryer, program, outcome2);

        // Assert
        Assert.Equal(2, _journal.Entries.Count);
        Assert.IsType(outcome1, Assert.IsType<JournalEntry.Dryer>(_journal.Entries[0]).Outcome);
        Assert.IsType(outcome2, Assert.IsType<JournalEntry.Dryer>(_journal.Entries[1]).Outcome);
    }

    [Theory]
    [MemberData(nameof(GetWasherDryerProgramCombinations))]
    public void Journal_WhenWasherAndDryerCyclesEnded_YieldsTwoEntries(
        WasherProgram washerProgram, DryerProgram dryerProgram)
    {
        // Arrange
        var washer = GetAttachedPoweredWasher();
        _clock.Advance(TimeSpan.FromMinutes(5));

        var dryer = GetAttachedPoweredDryer();
        _clock.Advance(TimeSpan.FromMinutes(5));

        // Act
        var washerStartedAt = _clock.GetUtcNow();
        washer.StartProgram(washerProgram);
        _clock.Advance(TimeSpan.FromMinutes(5));

        var dryerStartedAt = _clock.GetUtcNow();
        dryer.StartProgram(dryerProgram);

        _clock.Advance(washerProgram.Duration() / 10);
        
        var washerCutAt = _clock.GetUtcNow();
        washer.SwitchOff();
        _clock.Advance(dryerProgram.Duration() - washerProgram.Duration() / 10);

        // Assert
        Assert.Equal(2, _journal.Entries.Count);

        var washerEntry = Assert.IsType<JournalEntry.Washer>(_journal.Entries[0]);
        Assert.Equal(_washerId, washerEntry.WasherId);
        var washerOutcome = Assert.IsType<CycleOutcome.Interrupted>(washerEntry.Outcome);
        Assert.Equal(washerStartedAt, washerOutcome.StartedAt);
        Assert.Equal(washerCutAt, washerOutcome.InterruptedAt);

        var dryerEntry = Assert.IsType<JournalEntry.Dryer>(_journal.Entries[1]);
        Assert.Equal(_dryerId, dryerEntry.DryerId);
        var dryerOutcome = Assert.IsType<CycleOutcome.Completed>(dryerEntry.Outcome);
        Assert.Equal(dryerStartedAt, dryerOutcome.StartedAt);
        Assert.Equal(dryerStartedAt.Add(dryerProgram.Duration()), dryerOutcome.CompletedAt);
    }

    [Theory]
    [MemberData(nameof(GetWasherProgramWithOutcome))]
    public void Journal_WhenWasherCycleEnded_CalculatesEnergyUsed(WasherProgram washerProgram, Type outcome)
    {
        // Arrange
        var washer = GetAttachedPoweredWasher();
        var kwhBefore = Kwh.From(1.1m);
        var kwhAfter = Kwh.From(5.3m);
        var expectedElectricityConsumed = kwhAfter - kwhBefore;
        _meter.Read().Returns(MeterReading.From(kwhBefore), MeterReading.From(kwhAfter));

        // Act
        RunWasherCycleWithOutcome(washer, washerProgram, outcome);

        // Assert
        var washerEntry = Assert.IsType<JournalEntry.Washer>(_journal.Entries[0]);
        Assert.Equal(_washerId, washerEntry.WasherId);
        Assert.Equal(expectedElectricityConsumed, washerEntry.ElectricityConsumed);
    }

    [Theory]
    [MemberData(nameof(GetWasherProgramsWithTwoOutcomes))]
    public void Journal_WhenTwoWasherCyclesEnded_CalculatesEnergyUsed(
        WasherProgram washerProgram, Type outcome1, Type outcome2)
    {
        // Arrange
        var washer = GetAttachedPoweredWasher();
        var kwhBefore1 = Kwh.From(1.1m);
        var kwhAfter1 = Kwh.From(5.3m);
        var kwhBefore2 = Kwh.From(8.4m);
        var kwhAfter2 = Kwh.From(11.2m);
        var expectedElectricityConsumed1 = kwhAfter1 - kwhBefore1;
        var expectedElectricityConsumed2 = kwhAfter2 - kwhBefore2;
        _meter.Read().Returns(
            MeterReading.From(kwhBefore1), MeterReading.From(kwhAfter1),
            MeterReading.From(kwhBefore2), MeterReading.From(kwhAfter2));

        // Act
        RunWasherCycleWithOutcome(washer, washerProgram, outcome1);
        RunWasherCycleWithOutcome(washer, washerProgram, outcome2);

        // Assert
        Assert.Equal(2, _journal.Entries.Count);
        var washerEntry1 = Assert.IsType<JournalEntry.Washer>(_journal.Entries[0]);
        var washerEntry2 = Assert.IsType<JournalEntry.Washer>(_journal.Entries[1]);
        Assert.Equal(expectedElectricityConsumed1, washerEntry1.ElectricityConsumed);
        Assert.Equal(expectedElectricityConsumed2, washerEntry2.ElectricityConsumed);
    }




    [Theory]
    [MemberData(nameof(GetDryerProgramWithOutcome))]
    public void Journal_WhenDryerCycleEnded_CalculatesEnergyUsed(DryerProgram dryerProgram, Type outcome)
    {
        // Arrange
        var dryer = GetAttachedPoweredDryer();
        var kwhBefore = Kwh.From(1.1m);
        var kwhAfter = Kwh.From(5.3m);
        var expectedElectricityConsumed = kwhAfter - kwhBefore;
        _meter.Read().Returns(MeterReading.From(kwhBefore), MeterReading.From(kwhAfter));

        // Act
        RunDryerCycleWithOutcome(dryer, dryerProgram, outcome);

        // Assert
        var dryerEntry = Assert.IsType<JournalEntry.Dryer>(_journal.Entries[0]);
        Assert.Equal(_dryerId, dryerEntry.DryerId);
        Assert.Equal(expectedElectricityConsumed, dryerEntry.ElectricityConsumed);
    }

    [Theory]
    [MemberData(nameof(GetDryerProgramsWithTwoOutcomes))]
    public void Journal_WhenTwoDryerCyclesEnded_CalculatesEnergyUsed(
        DryerProgram dryerProgram, Type outcome1, Type outcome2)
    {
        // Arrange
        var dryer = GetAttachedPoweredDryer();
        var kwhBefore1 = Kwh.From(1.1m);
        var kwhAfter1 = Kwh.From(5.3m);
        var kwhBefore2 = Kwh.From(8.4m);
        var kwhAfter2 = Kwh.From(11.2m);
        var expectedElectricityConsumed1 = kwhAfter1 - kwhBefore1;
        var expectedElectricityConsumed2 = kwhAfter2 - kwhBefore2;
        _meter.Read().Returns(
            MeterReading.From(kwhBefore1), MeterReading.From(kwhAfter1),
            MeterReading.From(kwhBefore2), MeterReading.From(kwhAfter2));

        // Act
        RunDryerCycleWithOutcome(dryer, dryerProgram, outcome1);
        RunDryerCycleWithOutcome(dryer, dryerProgram, outcome2);

        // Assert
        Assert.Equal(2, _journal.Entries.Count);
        var dryerEntry1 = Assert.IsType<JournalEntry.Dryer>(_journal.Entries[0]);
        var dryerEntry2 = Assert.IsType<JournalEntry.Dryer>(_journal.Entries[1]);
        Assert.Equal(expectedElectricityConsumed1, dryerEntry1.ElectricityConsumed);
        Assert.Equal(expectedElectricityConsumed2, dryerEntry2.ElectricityConsumed);
    }

    private void RunWasherCycleWithOutcome(Washer washer, WasherProgram program, Type outcome)
    {
        washer.StartProgram(program);
        _clock.Advance(program.Duration() / 2);
        if (outcome == typeof(CycleOutcome.Completed)) _clock.Advance(program.Duration() / 2);
        else
        {
            washer.SwitchOff();
            washer.SwitchOn();
        }
    }

    private void RunDryerCycleWithOutcome(Dryer dryer, DryerProgram program, Type outcome)
    {
        dryer.StartProgram(program);
        _clock.Advance(program.Duration() / 2);
        if (outcome == typeof(CycleOutcome.Completed)) _clock.Advance(program.Duration() / 2);
        else
        {
            dryer.SwitchOff();
            dryer.SwitchOn();
        }
    }

    private Washer GetAttachedPoweredWasher()
    {
        _powerSource.IsPowerOn.Returns(true);
        var washer = new Washer(_washerId, _powerSource, _clock);
        _journal.Attach(washer, _meter);
        washer.SwitchOn();
        return washer;
    }

    private Dryer GetAttachedPoweredDryer()
    {
        _powerSource.IsPowerOn.Returns(true);
        var dryer = new Dryer(_dryerId, _powerSource, _clock);
        _journal.Attach(dryer, _meter);
        dryer.SwitchOn();
        return dryer;
    }

    private static MeterReading Reading(decimal value) => MeterReading.From(Kwh.From(value));

    public static IEnumerable<object[]> GetWasherProgramWithOutcome()
    {
        Type[] outcomes = { typeof(CycleOutcome.Completed), typeof(CycleOutcome.Interrupted) };
        foreach (var program in Enum.GetValues<WasherProgram>())
            foreach (var outcome in outcomes)
                yield return new object[] { program, outcome };
    }

    public static IEnumerable<object[]> GetDryerProgramWithOutcome()
    {
        Type[] outcomes = { typeof(CycleOutcome.Completed), typeof(CycleOutcome.Interrupted) };
        foreach (var program in Enum.GetValues<DryerProgram>())
            foreach (var outcome in outcomes)
                yield return new object[] { program, outcome };
    }

    public static IEnumerable<object[]> GetWasherProgramsWithTwoOutcomes()
    {
        Type[] outcomes = { typeof(CycleOutcome.Completed), typeof(CycleOutcome.Interrupted) };
        foreach (var program in Enum.GetValues<WasherProgram>())
            foreach (var first in outcomes)
                foreach (var second in outcomes)
                    yield return new object[] { program, first, second };
    }

    public static IEnumerable<object[]> GetDryerProgramsWithTwoOutcomes()
    {
        Type[] outcomes = { typeof(CycleOutcome.Completed), typeof(CycleOutcome.Interrupted) };
        foreach (var program in Enum.GetValues<DryerProgram>())
            foreach (var first in outcomes)
                foreach (var second in outcomes)
                    yield return new object[] { program, first, second };
    }

    public static IEnumerable<object[]> GetWasherDryerProgramCombinations()
    {
        foreach (var washerProgram in Enum.GetValues<WasherProgram>())
            foreach (var dryerProgram in Enum.GetValues<DryerProgram>())
                yield return new object[] { washerProgram, dryerProgram };
    }

    public static IEnumerable<object[]> GetWasherPrograms()
    {
        foreach (var program in Enum.GetValues<WasherProgram>())
            yield return new object[] { program };
    }

    public static IEnumerable<object[]> GetDryerPrograms()
    {
        foreach (var program in Enum.GetValues<DryerProgram>())
            yield return new object[] { program };
    }
}