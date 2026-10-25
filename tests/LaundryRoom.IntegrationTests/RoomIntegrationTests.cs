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
    private readonly Flat _owner;
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
        _card = new Card(Guid.NewGuid());
        _card.Assign(_owner);
        _washer = new Washer(Guid.NewGuid(), _room, _clock);
        _dryer = new Dryer(Guid.NewGuid(), _room, _clock);
        _journal.Attach(_washer, _meter);
        _journal.Attach(_dryer, _meter);
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

    private static MeterReading Reading(decimal value) => MeterReading.From(Kwh.From(value));

    public static IEnumerable<object[]> GetWasherDryerProgramCombinations()
    {
        foreach (var washerProgram in Enum.GetValues<WasherProgram>())
            foreach (var dryerProgram in Enum.GetValues<DryerProgram>())
                yield return new object[] { washerProgram, dryerProgram };
    }
}