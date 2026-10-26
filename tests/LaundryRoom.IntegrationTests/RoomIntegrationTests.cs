using Xunit;
using NSubstitute;
using Microsoft.Extensions.Time.Testing;

public class RoomIntegrationTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);

    private readonly FakeTimeProvider _clock = new(Start);
    private readonly Washer _washer;
    private readonly Dryer _dryer;
    private readonly Card _card;
    private readonly Card _anotherCard;
    private readonly Flat _owner;
    private readonly Flat _anotherOwner;
    private readonly Journal _journal;
    private readonly Room _room;
    private readonly IElectricityMeter _meter = Substitute.For<IElectricityMeter>();

    public RoomIntegrationTests()
    {
        _meter.Read().Returns(Reading(0));
        _journal = new Journal(_clock);
        var roomLabel = "green";
        _room = new Room(roomLabel);
        _owner = Flat.From("3B");
        _anotherOwner = Flat.From("2d");
        _card = new Card(Guid.NewGuid());
        _card.Assign(_owner);
        _anotherCard = new Card(Guid.NewGuid());
        _anotherCard.Assign(_anotherOwner);
        _washer = new Washer(Guid.NewGuid(), _room, _clock);
        _dryer = new Dryer(Guid.NewGuid(), _room, _clock);
        _journal.Attach(_washer, _meter, _room);
        _journal.Attach(_dryer, _meter, _room);
    }

    [Theory]
    [MemberData(nameof(GetWasherDryerProgramCombinations))]
    public void Room_WhenWasherAndDryerRunningAndCardEjected_YieldsTwoEntries(
        WasherProgram washerProgram, DryerProgram dryerProgram)
    {
        // Arrange
        _room.InsertCard(_card);
        _washer.SwitchOn();
        _dryer.SwitchOn();

        var washerStartedAt = _clock.GetUtcNow();
        _washer.StartProgram(washerProgram);
        _clock.Advance(TimeSpan.FromMinutes(5));

        var dryerStartedAt = _clock.GetUtcNow();
        _dryer.StartProgram(dryerProgram);

        _clock.Advance(washerProgram.Duration() / 10);

        // Act
        var cutAt = _clock.GetUtcNow();
        _room.EjectCard();

        // Assert
        Assert.Equal(2, _journal.Entries.Count);

        var washerEntry = _journal.Entries.OfType<JournalEntry.Washer>().Single();
        Assert.Equal(_washer.Id, washerEntry.WasherId);
        var washerOutcome = Assert.IsType<CycleOutcome.Interrupted>(washerEntry.Outcome);
        Assert.Equal(washerStartedAt, washerOutcome.StartedAt);
        Assert.Equal(cutAt, washerOutcome.InterruptedAt);

        var dryerEntry =  _journal.Entries.OfType<JournalEntry.Dryer>().Single();
        Assert.Equal(_dryer.Id, dryerEntry.DryerId);
        var dryerOutcome = Assert.IsType<CycleOutcome.Interrupted>(dryerEntry.Outcome);
        Assert.Equal(dryerStartedAt, dryerOutcome.StartedAt);
        Assert.Equal(cutAt, dryerOutcome.InterruptedAt);
    }

    [Fact]
    public void Room_WhenAssignedCardInsertedAndNoApplianceRuns_YieldsNoEntries()
    {
        // Arrange
        _room.InsertCard(_card);
        _washer.SwitchOn();
        _dryer.SwitchOn();

        _clock.Advance(TimeSpan.FromHours(5));

        // Act
        _room.EjectCard();

        // Assert
        Assert.Empty(_journal.Entries);
    }

    [Theory]
    [MemberData(nameof(GetWasherProgramWithOutcome))]
    public void Room_WhenWasherCycleEnded_YieldsEntryWithCardOwner(WasherProgram washerProgram, Type outcome)
    {
        // Arrange
        _room.InsertCard(_card);
        _washer.SwitchOn();

        // Act
        RunWasherCycleWithOutcome(_washer, washerProgram, outcome);

        // Assert
        var washerEntry = _journal.Entries.OfType<JournalEntry.Washer>().Single();
        Assert.Equal(_card.Owner, washerEntry.CardOwner);
    }

    [Theory]
    [MemberData(nameof(GetWasherPrograms))]
    public void Room_WhenTwoOwnersRunTwoWasherCycles_YieldsTwoEntries(WasherProgram washerProgram)
    {
        // Arrange
        // Act
        _room.InsertCard(_card);
        _washer.SwitchOn();
        RunWasherCycleWithOutcome(_washer, washerProgram, typeof(CycleOutcome.Interrupted));

        _room.InsertCard(_anotherCard);
        _washer.SwitchOn();
        RunWasherCycleWithOutcome(_washer, washerProgram, typeof(CycleOutcome.Completed));

        // Assert
        Assert.Equal(2, _journal.Entries.Count);
        var washerEntry1 = Assert.IsType<JournalEntry.Washer>(_journal.Entries[0]);
        var washerEntry2 = Assert.IsType<JournalEntry.Washer>(_journal.Entries[1]);
        Assert.Equal(_owner, washerEntry1.CardOwner);
        Assert.Equal(_anotherOwner, washerEntry2.CardOwner);
    }

    [Theory]
    [MemberData(nameof(GetDryerProgramWithOutcome))]
    public void Room_WhenDryerCycleEnded_YieldsEntryWithCardOwner(DryerProgram dryerProgram, Type outcome)
    {
        // Arrange
        _room.InsertCard(_card);
        _dryer.SwitchOn();

        // Act
        RunDryerCycleWithOutcome(_dryer, dryerProgram, outcome);

        // Assert
        var dryerEntry = _journal.Entries.OfType<JournalEntry.Dryer>().Single();
        Assert.Equal(_card.Owner, dryerEntry.CardOwner);
    }

    [Theory]
    [MemberData(nameof(GetDryerPrograms))]
    public void Room_WhenTwoOwnersRunTwoDryerCycles_YieldsTwoEntries(DryerProgram dryerProgram)
    {
        // Arrange
        // Act
        _room.InsertCard(_card);
        _dryer.SwitchOn();
        RunDryerCycleWithOutcome(_dryer, dryerProgram, typeof(CycleOutcome.Interrupted));

        _room.InsertCard(_anotherCard);
        _dryer.SwitchOn();
        RunDryerCycleWithOutcome(_dryer, dryerProgram, typeof(CycleOutcome.Completed));

        // Assert
        Assert.Equal(2, _journal.Entries.Count);
        var dryerEntry1 = Assert.IsType<JournalEntry.Dryer>(_journal.Entries[0]);
        var dryerEntry2 = Assert.IsType<JournalEntry.Dryer>(_journal.Entries[1]);
        Assert.Equal(_owner, dryerEntry1.CardOwner);
        Assert.Equal(_anotherOwner, dryerEntry2.CardOwner);
    }

    private static MeterReading Reading(decimal value) => MeterReading.From(Kwh.From(value));

    private void RunWasherCycleWithOutcome(Washer washer, WasherProgram program, Type outcome)
    {
        washer.StartProgram(program);
        _clock.Advance(program.Duration() / 2);
        if (outcome == typeof(CycleOutcome.Completed)) _clock.Advance(program.Duration() / 2);
        else
        {
            _room.EjectCard();
        }
    }

    private void RunDryerCycleWithOutcome(Dryer dryer, DryerProgram program, Type outcome)
    {
        dryer.StartProgram(program);
        _clock.Advance(program.Duration() / 2);
        if (outcome == typeof(CycleOutcome.Completed)) _clock.Advance(program.Duration() / 2);
        else
        {
            _room.EjectCard();
        }
    }

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