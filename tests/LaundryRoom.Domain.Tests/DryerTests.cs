using Xunit;
using NSubstitute;
using Microsoft.Extensions.Time.Testing;

public class DryerTests
{
    private IPowerSource _powerSource;
    private FakeTimeProvider _clock;
    public DryerTests()
    {
        _powerSource = Substitute.For<IPowerSource>();
        _clock = new FakeTimeProvider();
    }

    [Fact]
    public void Dryer_WhenCreatedWithValidId_IsOff()
    {
        // Arrange
        // Act
        var dryer = new Dryer(Guid.NewGuid(), _powerSource, _clock);

        // Assert
        Assert.False(dryer.IsOn);
    }

    [Fact]
    public void Dryer_WhenCreatedWithValidId_IsIdle()
    {
        // Arrange
        // Act
        var dryer = new Dryer(Guid.NewGuid(), _powerSource, _clock);

        // Assert
        Assert.IsType<DryerState.Idle>(dryer.State);
    }

    [Fact]
    public void Dryer_WhenCreatedWithInalidId_Throws()
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentException>(() => new Dryer(Guid.Empty, _powerSource, _clock));
    }

    [Fact]
    public void Dryer_WhenCreatedWithNoPowerSource_Throws()
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentNullException>(() => new Dryer(Guid.NewGuid(), null!, _clock));
    }

    [Fact]
    public void Dryer_WhenPoweredAndSwitchOnCalled_IsOn()
    {
        // Arrange
        var dryer = GetPoweredDryer();

        // Act
        dryer.SwitchOn();

        // Assert
        Assert.True(dryer.IsOn);
    }

    [Fact]
    public void Dryer_WhenNotPoweredAndSwitchOnCalled_IsOff()
    {
        // Arrange
        var dryer = new Dryer(Guid.NewGuid(), _powerSource, _clock);

        // Act
        dryer.SwitchOn();

        // Assert
        Assert.False(dryer.IsOn);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Dryer_WhenOnAndStartCalled_IsRunning(DryerProgram program)
    {
        // Arrange
        var dryer = GetPoweredDryer();
        dryer.SwitchOn();

        // Act
        dryer.StartProgram(program);

        // Assert
        var running = Assert.IsType<DryerState.Running>(dryer.State);
        Assert.Equal(program, running.Program);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Dryer_WhenOffAndStartCalled_Throws(DryerProgram program)
    {
        // Arrange
        var dryer = GetPoweredDryer();

        // Act
        // Assert
        Assert.Throws<InvalidOperationException>(() => dryer.StartProgram(program));
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Dryer_WhenPowerLostAndStartCalled_Throws(DryerProgram program)
    {
        // Arrange
        var dryer = GetPoweredDryer();
        dryer.SwitchOn();
        _powerSource.IsPowerOn.Returns(false);

        // Act
        // Assert
        Assert.Throws<InvalidOperationException>(() => dryer.StartProgram(program));
    }
    
    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Dryer_WhenRunningAndStartCalled_Throws(DryerProgram program)
    {
        // Arrange
        var dryer = GetPoweredDryer();
        dryer.SwitchOn();
        dryer.StartProgram(program);

        // Act
        // Assert
        Assert.Throws<InvalidOperationException>(() => dryer.StartProgram(program));
    }

    [Fact]
    public void Dryer_WhenOnAndSwitchOffCalled_IsOff()
    {
        // Arrange
        var dryer = GetPoweredDryer();
        dryer.SwitchOn();

        // Act
        dryer.SwitchOff();

        // Assert
        Assert.False(dryer.IsOn);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Dryer_WhenRunningAndSwitchOffCalled_IsOff(DryerProgram program)
    {

        // Arrange
        var dryer = GetPoweredDryer();
        dryer.SwitchOn();
        dryer.StartProgram(program);

        // Act
        dryer.SwitchOff();

        // Assert
        Assert.IsType<DryerState.Idle>(dryer.State);
        Assert.False(dryer.IsOn);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Dryer_WhenRunningAndPowerLost_IsOff(DryerProgram program)
    {

        // Arrange
        var dryer = GetPoweredDryer();
        dryer.SwitchOn();
        dryer.StartProgram(program);

        // Act
        _powerSource.IsPowerOn.Returns(false);
        _powerSource.PowerOff += Raise.Event<Action>();

        // Assert
        Assert.IsType<DryerState.Idle>(dryer.State);
        Assert.False(dryer.IsOn);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Dryer_WhenRunningAndPowerLostAndBackOn_IsOff(DryerProgram program)
    {

        // Arrange
        var dryer = GetPoweredDryer();
        dryer.SwitchOn();
        dryer.StartProgram(program);
        _powerSource.IsPowerOn.Returns(false);
        _powerSource.PowerOff += Raise.Event<Action>();

        // Act
        _powerSource.IsPowerOn.Returns(true);
        _powerSource.PowerOn += Raise.Event<Action>();

        // Assert
        Assert.IsType<DryerState.Idle>(dryer.State);
        Assert.False(dryer.IsOn);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Dryer_WhenProgramDurationElapsed_IsIdle(DryerProgram program)
    {
        // Arrange
        var dryer = GetPoweredDryer();
        dryer.SwitchOn();
        dryer.StartProgram(program);

        // Act
        _clock.Advance(program.Duration());

        // Assert
        Assert.IsType<DryerState.Idle>(dryer.State);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Dryer_WhenProgramDurationNotElapsed_IsRunningSameProgram(DryerProgram program)
    {
        // Arrange
        var dryer = GetPoweredDryer();
        dryer.SwitchOn();
        dryer.StartProgram(program);

        // Act
        _clock.Advance(program.Duration().Subtract(TimeSpan.FromMinutes(1)));

        // Assert
        var running = Assert.IsType<DryerState.Running>(dryer.State);
        Assert.Equal(program, running.Program);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Dryer_WhenRunning_MeasuresElapsedFromProgramStart(DryerProgram program)
    {
        // Arrange
        var duration = program.Duration();
        var dryer = GetPoweredDryer();
        dryer.SwitchOn();
        _clock.Advance(duration);
        dryer.StartProgram(program);
        var remainingMins = TimeSpan.FromMinutes(1);

        // Act
        _clock.Advance(duration.Subtract(remainingMins));

        // Assert
        var running = Assert.IsType<DryerState.Running>(dryer.State);
        Assert.Equal(program, running.Program);
        _clock.Advance(remainingMins);
        Assert.IsType<DryerState.Idle>(dryer.State);
    }

    [Theory]
    [MemberData(nameof(GetProgramPairs))]
    public void Dryer_WhenRunningSwitchedOffAndStartAgain_MeasuresElapsedFromNewStart(
        DryerProgram program1, DryerProgram program2)
    {
        // Arrange
        var dryer = GetPoweredDryer();
        dryer.SwitchOn();
        dryer.StartProgram(program1);
        _clock.Advance(program1.Duration() / 2);
        dryer.SwitchOff();
        dryer.SwitchOn();
        dryer.StartProgram(program2);

        // Act
        _clock.Advance(program1.Duration() / 2);

        // Assert
        var running = Assert.IsType<DryerState.Running>(dryer.State);
        Assert.Equal(program2, running.Program);
        _clock.Advance(program2.Duration() - program1.Duration() / 2);
        Assert.IsType<DryerState.Idle>(dryer.State);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Dryer_WhenStarted_ReportsStarted(DryerProgram program)
    {
        // Arrange
        var started = new List<DryerProgram>();
        var dryer = GetPoweredDryer();
        dryer.ProgramStarted += p => started.Add(p);
        dryer.SwitchOn();

        // Act
        dryer.StartProgram(program);

        // Assert
        Assert.Equal(new[] { program }, started);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Dryer_WhenProgramDurationElapsed_ReportsCompleted(DryerProgram program)
    {
        // Arrange
        var completed = new List<DryerProgram>();
        var dryer = GetPoweredDryer();
        dryer.ProgramCompleted += p => completed.Add(p);
        dryer.SwitchOn();
        dryer.StartProgram(program);

        // Act
        _clock.Advance(program.Duration());

        // Assert
        Assert.Equal(new[] { program }, completed);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Dryer_WhenProgramDurationNotElapsed_DoesNotReportCompleted(DryerProgram program)
    {
        // Arrange
        var completed = new List<DryerProgram>();
        var started = new List<DryerProgram>();
        var dryer = GetPoweredDryer();
        dryer.ProgramCompleted += p => completed.Add(p);
        dryer.ProgramStarted += p => started.Add(p);
        dryer.SwitchOn();
        dryer.StartProgram(program);

        // Act
        _clock.Advance(program.Duration().Subtract(TimeSpan.FromMinutes(1)));

        // Assert
        Assert.Equal(new[] { program }, started);
        Assert.Empty(completed);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Dryer_WhenHoursPassedAfterProgramDurationElapsed_StillSingleReport(DryerProgram program)
    {
        // Arrange
        var completed = new List<DryerProgram>();
        var dryer = GetPoweredDryer();
        dryer.ProgramCompleted += p => completed.Add(p);
        dryer.SwitchOn();
        dryer.StartProgram(program);

        // Act
        _clock.Advance(program.Duration() * 5);

        // Assert
        Assert.Equal(new[] { program }, completed);
    }

    [Theory]
    [MemberData(nameof(GetProgramPairs))]
    public void Dryer_WhenTwoDifferentCyclesCompletedBackToBack_ReportsExactlyTwiceInOrder(
        DryerProgram program1, DryerProgram program2)
    {
        // Arrange
        var completed = new List<DryerProgram>();
        var dryer = GetPoweredDryer();
        dryer.ProgramCompleted += p => completed.Add(p);
        dryer.SwitchOn();

        // Act
        dryer.StartProgram(program1);
        _clock.Advance(program1.Duration());
        dryer.StartProgram(program2);
        _clock.Advance(program2.Duration());

        // Assert
        Assert.Equal(new[] { program1, program2 }, completed);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Dryer_WhenTwoSameCyclesCompletedBackToBack_ReportsExactlyTwiceInOrder(DryerProgram program)
    {
        // Arrange
        var completed = new List<DryerProgram>();
        var dryer = GetPoweredDryer();
        dryer.ProgramCompleted += p => completed.Add(p);
        dryer.SwitchOn();

        // Act
        dryer.StartProgram(program);
        _clock.Advance(program.Duration());
        dryer.StartProgram(program);
        _clock.Advance(program.Duration());

        // Assert
        Assert.Equal(new[] { program, program }, completed);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Dryer_WhenSwitchedOffMidCycleAndHoursPassed_DoesNotReportCompleted(DryerProgram program)
    {
        // Arrange
        var completed = new List<DryerProgram>();
        var started = new List<DryerProgram>();
        var dryer = GetPoweredDryer();
        dryer.ProgramCompleted += p => completed.Add(p);
        dryer.ProgramStarted += p => started.Add(p);
        dryer.SwitchOn();
        dryer.StartProgram(program);
        _clock.Advance(program.Duration() / 2);

        // Act
        dryer.SwitchOff();
        _clock.Advance(program.Duration() * 5);

        // Assert
        Assert.Equal(new[] { program }, started);
        Assert.Empty(completed);
    }
    
    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Dryer_WhenRunningAndSwitchOffCalled_ReportsInterrupted(DryerProgram program)
    {
        // Arrange
        var interrupted = new List<DryerProgram>();
        var dryer = GetPoweredDryer();
        dryer.ProgramInterrupted += p => interrupted.Add(p);
        dryer.SwitchOn();
        dryer.StartProgram(program);

        // Act
        dryer.SwitchOff();

        // Assert
        Assert.Equal(new[] { program }, interrupted);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Dryer_WhenRunningAndPowerLost_ReportsInterrupted(DryerProgram program)
    {
        // Arrange
        var interrupted = new List<DryerProgram>();
        var dryer = GetPoweredDryer();
        dryer.ProgramInterrupted += p => interrupted.Add(p);
        dryer.SwitchOn();
        dryer.StartProgram(program);
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
    public void Dryer_WhenInterruptedAndHoursPassed_DoesNotReportCompleted(DryerProgram program)
    {
        // Arrange
        var interrupted = new List<DryerProgram>();
        var completed = new List<DryerProgram>();
        var dryer = GetPoweredDryer();
        dryer.ProgramInterrupted += p => interrupted.Add(p);
        dryer.ProgramCompleted += p => completed.Add(p);
        dryer.SwitchOn();
        dryer.StartProgram(program);
        dryer.SwitchOff();

        // Act
        _clock.Advance(program.Duration() * 5);

        // Assert
        Assert.Equal(new[] { program }, interrupted);
        Assert.Empty(completed);
    }

    [Fact]
    public void Dryer_WhenIdleAndSwitchOffCalled_DoesNotReportInterrupted()
    {
        // Arrange
        var interrupted = new List<DryerProgram>();
        var dryer = GetPoweredDryer();
        dryer.ProgramInterrupted += p => interrupted.Add(p);
        dryer.SwitchOn();

        // Act
        dryer.SwitchOff();

        // Assert
        Assert.Empty(interrupted);
    }

    private Dryer GetPoweredDryer()
    {
        var dryer = new Dryer(Guid.NewGuid(), _powerSource, _clock);
        _powerSource.IsPowerOn.Returns(true);

        return dryer;
    }

    public static IEnumerable<object[]> GetPrograms()
    {
        var programs = Enum.GetValues<DryerProgram>();
        for (int i = 0; i < programs.Length; i++)
        {
            yield return new object[] { programs[i] };
        }
    }

    public static IEnumerable<object[]> GetProgramPairs()
    {
        var programs = Enum.GetValues<DryerProgram>();
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