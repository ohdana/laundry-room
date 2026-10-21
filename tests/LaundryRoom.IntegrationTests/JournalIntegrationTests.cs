using Xunit;
using NSubstitute;
using Microsoft.Extensions.Time.Testing;

public class JournalIntegrationTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);

    private readonly FakeTimeProvider _clock = new(Start);
    private readonly IPowerSource _powerSource = Substitute.For<IPowerSource>();
    private readonly Guid _washerId = Guid.NewGuid();
    private readonly Journal _journal;
    
    public JournalIntegrationTests()
    {
        _journal = new Journal(_clock);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Journal_WhenAttachedToWasherAndWasherStarted_YieldsNoEntry(WasherProgram program)
    {
        // Arrange
        var washer = GetAttachedPoweredWasher();
        _clock.Advance(TimeSpan.FromMinutes(5));

        // Act
        washer.StartProgram(program);
        _clock.Advance(program.Duration() / 2);

        // Assert
        Assert.Equal(0, _journal.Entries.Count);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
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
    [MemberData(nameof(GetPrograms))]
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
    [MemberData(nameof(GetProgramsWithTwoOutcomes))]
    public void Journal_WhenTwoWasherCyclesEnded_YieldsTwoEntriesInOrder(
        WasherProgram program, Type outcome1, Type outcome2)
    {
        // Arrange
        var washer = GetAttachedPoweredWasher();
        _clock.Advance(TimeSpan.FromMinutes(5));

        // Act
        RunCycleWithOutcome(washer, program, outcome1);
        RunCycleWithOutcome(washer, program, outcome2);

        // Assert
        Assert.Equal(2, _journal.Entries.Count);
        Assert.IsType(outcome1, Assert.IsType<JournalEntry.Washer>(_journal.Entries[0]).Outcome);
        Assert.IsType(outcome2, Assert.IsType<JournalEntry.Washer>(_journal.Entries[1]).Outcome);
    }

    private void RunCycleWithOutcome(Washer washer, WasherProgram program, Type outcome)
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

    private Washer GetAttachedPoweredWasher()
    {
        _powerSource.IsPowerOn.Returns(true);
        var washer = new Washer(_washerId, _powerSource, _clock);
        _journal.Attach(washer);
        washer.SwitchOn();
        return washer;
    }

    public static IEnumerable<object[]> GetProgramsWithTwoOutcomes()
    {
        var programs = Enum.GetValues<WasherProgram>();
        Type[] outcomes = { typeof(CycleOutcome.Completed), typeof(CycleOutcome.Interrupted) };
        foreach (var program in programs)
        {
            foreach (var first in outcomes)
            {
                foreach (var second in outcomes)
                {
                    yield return new object[] { program, first, second };
                }
            }
        }
    }

    public static IEnumerable<object[]> GetPrograms()
    {
        var programs = Enum.GetValues<WasherProgram>();
        for (int i = 0; i < programs.Length; i++)
        {
            yield return new object[] { programs[i] };
        }
    }
}