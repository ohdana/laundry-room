using Xunit;
using NSubstitute;
using Microsoft.Extensions.Time.Testing;

public class WasherTests
{
    private IPowerSource _powerSource;
    private FakeTimeProvider _clock;
    public WasherTests()
    {
        _powerSource = Substitute.For<IPowerSource>();
        _clock = new FakeTimeProvider();
    }

    [Fact]
    public void Washer_WhenCreatedWithValidId_IsOff()
    {
        // Arrange
        // Act
        var washer = new Washer(Guid.NewGuid(), _powerSource, _clock);

        // Assert
        Assert.False(washer.IsOn);
    }

    [Fact]
    public void Washer_WhenCreatedWithValidId_IsIdle()
    {
        // Arrange
        // Act
        var washer = new Washer(Guid.NewGuid(), _powerSource, _clock);

        // Assert
        Assert.IsType<WasherState.Idle>(washer.State);
    }

    [Fact]
    public void Washer_WhenCreatedWithInalidId_Throws()
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentException>(() => new Washer(Guid.Empty, _powerSource, _clock));
    }

    [Fact]
    public void Washer_WhenCreatedWithNoPowerSource_Throws()
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentNullException>(() => new Washer(Guid.NewGuid(), null!, _clock));
    }

    [Fact]
    public void Washer_WhenPoweredAndSwitchOnCalled_IsOn()
    {
        // Arrange
        var washer = GetPoweredWasher();

        // Act
        washer.SwitchOn();

        // Assert
        Assert.True(washer.IsOn);
    }

    [Fact]
    public void Washer_WhenNotPoweredAndSwitchOnCalled_IsOff()
    {
        // Arrange
        var washer = new Washer(Guid.NewGuid(), _powerSource, _clock);

        // Act
        washer.SwitchOn();

        // Assert
        Assert.False(washer.IsOn);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Washer_WhenOnAndStartCalled_IsRunning(WasherProgram program)
    {
        // Arrange
        var washer = GetPoweredWasher();
        washer.SwitchOn();

        // Act
        washer.StartProgram(program);

        // Assert
        var running = Assert.IsType<WasherState.Running>(washer.State);
        Assert.Equal(program, running.Program);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Washer_WhenOffAndStartCalled_Throws(WasherProgram program)
    {
        // Arrange
        var washer = GetPoweredWasher();

        // Act
        // Assert
        Assert.Throws<InvalidOperationException>(() => washer.StartProgram(program));
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Washer_WhenPowerLostAndStartCalled_Throws(WasherProgram program)
    {
        // Arrange
        var washer = GetPoweredWasher();
        washer.SwitchOn();
        _powerSource.IsPowerOn.Returns(false);

        // Act
        // Assert
        Assert.Throws<InvalidOperationException>(() => washer.StartProgram(program));
    }
    
    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Washer_WhenRunningAndStartCalled_Throws(WasherProgram program)
    {
        // Arrange
        var washer = GetPoweredWasher();
        washer.SwitchOn();
        washer.StartProgram(program);

        // Act
        // Assert
        Assert.Throws<InvalidOperationException>(() => washer.StartProgram(program));
    }

    [Fact]
    public void Washer_WhenOnAndSwitchOffCalled_IsOff()
    {
        // Arrange
        var washer = GetPoweredWasher();
        washer.SwitchOn();

        // Act
        washer.SwitchOff();

        // Assert
        Assert.False(washer.IsOn);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Washer_WhenRunningAndSwitchOffCalled_IsOff(WasherProgram program)
    {

        // Arrange
        var washer = GetPoweredWasher();
        washer.SwitchOn();
        washer.StartProgram(program);

        // Act
        washer.SwitchOff();

        // Assert
        Assert.IsType<WasherState.Idle>(washer.State);
        Assert.False(washer.IsOn);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Washer_WhenRunningAndPowerLost_IsOff(WasherProgram program)
    {

        // Arrange
        var washer = GetPoweredWasher();
        washer.SwitchOn();
        washer.StartProgram(program);

        // Act
        _powerSource.IsPowerOn.Returns(false);
        _powerSource.PowerOff += Raise.Event<Action>();

        // Assert
        Assert.IsType<WasherState.Idle>(washer.State);
        Assert.False(washer.IsOn);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Washer_WhenRunningAndPowerLostAndBackOn_IsOff(WasherProgram program)
    {
        // Arrange
        var washer = GetPoweredWasher();
        washer.SwitchOn();
        washer.StartProgram(program);
        _powerSource.IsPowerOn.Returns(false);
        _powerSource.PowerOff += Raise.Event<Action>();

        // Act
        _powerSource.IsPowerOn.Returns(true);
        _powerSource.PowerOn += Raise.Event<Action>();

        // Assert
        Assert.IsType<WasherState.Idle>(washer.State);
        Assert.False(washer.IsOn);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Washer_WhenProgramDurationElapsed_IsIdle(WasherProgram program)
    {
        // Arrange
        var washer = GetPoweredWasher();
        washer.SwitchOn();
        washer.StartProgram(program);

        // Act
        _clock.Advance(program.Duration());

        // Assert
        Assert.IsType<WasherState.Idle>(washer.State);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Washer_WhenProgramDurationNotElapsed_IsRunningSameProgram(WasherProgram program)
    {
        // Arrange
        var washer = GetPoweredWasher();
        washer.SwitchOn();
        washer.StartProgram(program);

        // Act
        _clock.Advance(program.Duration().Subtract(TimeSpan.FromMinutes(1)));

        // Assert
        var running = Assert.IsType<WasherState.Running>(washer.State);
        Assert.Equal(program, running.Program);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Washer_WhenRunning_MeasuresElapsedFromProgramStart(WasherProgram program)
    {
        // Arrange
        var duration = program.Duration();
        var washer = GetPoweredWasher();
        washer.SwitchOn();
        _clock.Advance(duration);
        washer.StartProgram(program);
        var remainingMins = TimeSpan.FromMinutes(1);

        // Act
        _clock.Advance(duration.Subtract(remainingMins));

        // Assert
        var running = Assert.IsType<WasherState.Running>(washer.State);
        Assert.Equal(program, running.Program);
        _clock.Advance(remainingMins);
        Assert.IsType<WasherState.Idle>(washer.State);
    }

    [Theory]
    [MemberData(nameof(GetProgramPairs))]
    public void Washer_WhenRunningSwitchedOffAndStartAgain_MeasuresElapsedFromNewStart(
        WasherProgram program1, WasherProgram program2)
    {
        // Arrange
        var washer = GetPoweredWasher();
        washer.SwitchOn();
        washer.StartProgram(program1);
        _clock.Advance(program1.Duration() / 2);
        washer.SwitchOff();
        washer.SwitchOn();
        washer.StartProgram(program2);

        // Act
        _clock.Advance(program1.Duration() / 2);

        // Assert
        var running = Assert.IsType<WasherState.Running>(washer.State);
        Assert.Equal(program2, running.Program);
        _clock.Advance(program2.Duration() - program1.Duration() / 2);
        Assert.IsType<WasherState.Idle>(washer.State);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Washer_WhenStarted_ReportsStarted(WasherProgram program)
    {
        // Arrange
        var started = new List<WasherProgram>();
        var washer = GetPoweredWasher();
        washer.ProgramStarted += p => started.Add(p);
        washer.SwitchOn();

        // Act
        washer.StartProgram(program);

        // Assert
        Assert.Equal(new[] { program }, started);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Washer_WhenProgramDurationElapsed_ReportsCompleted(WasherProgram program)
    {
        // Arrange
        var completed = new List<WasherProgram>();
        var washer = GetPoweredWasher();
        washer.ProgramCompleted += p => completed.Add(p);
        washer.SwitchOn();
        washer.StartProgram(program);

        // Act
        _clock.Advance(program.Duration());

        // Assert
        Assert.Equal(new[] { program }, completed);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Washer_WhenProgramDurationNotElapsed_DoesNotReportCompleted(WasherProgram program)
    {
        // Arrange
        var completed = new List<WasherProgram>();
        var started = new List<WasherProgram>();
        var washer = GetPoweredWasher();
        washer.ProgramCompleted += p => completed.Add(p);
        washer.ProgramStarted += p => started.Add(p);
        washer.SwitchOn();
        washer.StartProgram(program);

        // Act
        _clock.Advance(program.Duration().Subtract(TimeSpan.FromMinutes(1)));

        // Assert
        Assert.Equal(new[] { program }, started);
        Assert.Empty(completed);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Washer_WhenHoursPassedAfterProgramDurationElapsed_StillSingleReport(WasherProgram program)
    {
        // Arrange
        var completed = new List<WasherProgram>();
        var washer = GetPoweredWasher();
        washer.ProgramCompleted += p => completed.Add(p);
        washer.SwitchOn();
        washer.StartProgram(program);

        // Act
        _clock.Advance(program.Duration() * 5);

        // Assert
        Assert.Equal(new[] { program }, completed);
    }

    [Theory]
    [MemberData(nameof(GetProgramPairs))]
    public void Washer_WhenTwoDifferentCyclesCompletedBackToBack_ReportsExactlyTwiceInOrder(
        WasherProgram program1, WasherProgram program2)
    {
        // Arrange
        var completed = new List<WasherProgram>();
        var washer = GetPoweredWasher();
        washer.ProgramCompleted += p => completed.Add(p);
        washer.SwitchOn();

        // Act
        washer.StartProgram(program1);
        _clock.Advance(program1.Duration());
        washer.StartProgram(program2);
        _clock.Advance(program2.Duration());

        // Assert
        Assert.Equal(new[] { program1, program2 }, completed);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Washer_WhenTwoSameCyclesCompletedBackToBack_ReportsExactlyTwiceInOrder(WasherProgram program)
    {
        // Arrange
        var completed = new List<WasherProgram>();
        var washer = GetPoweredWasher();
        washer.ProgramCompleted += p => completed.Add(p);
        washer.SwitchOn();

        // Act
        washer.StartProgram(program);
        _clock.Advance(program.Duration());
        washer.StartProgram(program);
        _clock.Advance(program.Duration());

        // Assert
        Assert.Equal(new[] { program, program }, completed);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Washer_WhenSwitchedOffMidCycleAndHoursPassed_DoesNotReportCompleted(WasherProgram program)
    {
        // Arrange
        var completed = new List<WasherProgram>();
        var started = new List<WasherProgram>();
        var washer = GetPoweredWasher();
        washer.ProgramCompleted += p => completed.Add(p);
        washer.ProgramStarted += p => started.Add(p);
        washer.SwitchOn();
        washer.StartProgram(program);
        _clock.Advance(program.Duration() / 2);

        // Act
        washer.SwitchOff();
        _clock.Advance(program.Duration() * 5);

        // Assert
        Assert.Equal(new[] { program }, started);
        Assert.Empty(completed);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Washer_WhenRunningAndSwitchOffCalled_ReportsInterrupted(WasherProgram program)
    {
        // Arrange
        var interrupted = new List<WasherProgram>();
        var washer = GetPoweredWasher();
        washer.ProgramInterrupted += p => interrupted.Add(p);
        washer.SwitchOn();
        washer.StartProgram(program);

        // Act
        washer.SwitchOff();

        // Assert
        Assert.Equal(new[] { program }, interrupted);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Washer_WhenRunningAndPowerLost_ReportsInterrupted(WasherProgram program)
    {
        // Arrange
        var interrupted = new List<WasherProgram>();
        var washer = GetPoweredWasher();
        washer.ProgramInterrupted += p => interrupted.Add(p);
        washer.SwitchOn();
        washer.StartProgram(program);
        _powerSource.IsPowerOn.Returns(false);
        _powerSource.PowerOff += Raise.Event<Action>();

        // Act
        _powerSource.IsPowerOn.Returns(true);
        _powerSource.PowerOn += Raise.Event<Action>();

        // Assert
        Assert.Equal(new[] { program }, interrupted);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Washer_WhenInterruptedAndHoursPassed_DoesNotReportCompleted(WasherProgram program)
    {
        // Arrange
        var interrupted = new List<WasherProgram>();
        var completed = new List<WasherProgram>();
        var washer = GetPoweredWasher();
        washer.ProgramInterrupted += p => interrupted.Add(p);
        washer.ProgramCompleted += p => completed.Add(p);
        washer.SwitchOn();
        washer.StartProgram(program);
        washer.SwitchOff();

        // Act
        _clock.Advance(program.Duration() * 5);

        // Assert
        Assert.Equal(new[] { program }, interrupted);
        Assert.Empty(completed);
    }

    [Fact]
    public void Washer_WhenIdleAndSwitchOffCalled_DoesNotReportInterrupted()
    {
        // Arrange
        var interrupted = new List<WasherProgram>();
        var washer = GetPoweredWasher();
        washer.ProgramInterrupted += p => interrupted.Add(p);
        washer.SwitchOn();

        // Act
        washer.SwitchOff();

        // Assert
        Assert.Empty(interrupted);
    }

    private Washer GetPoweredWasher()
    {
        var washer = new Washer(Guid.NewGuid(), _powerSource, _clock);
        _powerSource.IsPowerOn.Returns(true);

        return washer;
    }

    public static IEnumerable<object[]> GetPrograms()
    {
        var programs = Enum.GetValues<WasherProgram>();
        for (int i = 0; i < programs.Length; i++)
        {
            yield return new object[] { programs[i] };
        }
    }

    public static IEnumerable<object[]> GetProgramPairs()
    {
        var programs = Enum.GetValues<WasherProgram>();
        for (int i = 0; i < programs.Length; i++)
        {
            for (int j = 0; j < programs.Length; j++)
            {
                if (i == j) continue;
                yield return new object[] { programs[i], programs[j] };
            }
        }
    }
}