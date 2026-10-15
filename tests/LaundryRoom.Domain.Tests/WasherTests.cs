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
}