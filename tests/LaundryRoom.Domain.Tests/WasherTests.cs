using Xunit;
using NSubstitute;

public class WasherTests
{
    private IPowerSource _powerSource;
    public WasherTests() => _powerSource = Substitute.For<IPowerSource>();

    [Fact]
    public void Washer_WhenCreatedWithValidId_IsOff()
    {
        // Arrange
        // Act
        var washer = new Washer(Guid.NewGuid(), _powerSource);

        // Assert
        Assert.False(washer.IsOn);
    }

    [Fact]
    public void Washer_WhenCreatedWithValidId_IsIdle()
    {
        // Arrange
        // Act
        var washer = new Washer(Guid.NewGuid(), _powerSource);

        // Assert
        Assert.IsType<WasherState.Idle>(washer.State);
    }

    [Fact]
    public void Washer_WhenCreatedWithInalidId_Throws()
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentException>(() => new Washer(Guid.Empty, _powerSource));
    }

    [Fact]
    public void Washer_WhenCreatedWithNoPowerSource_Throws()
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentNullException>(() => new Washer(Guid.NewGuid(), null!));
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
        var washer = new Washer(Guid.NewGuid(), _powerSource);

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

    private Washer GetPoweredWasher()
    {
        var washer = new Washer(Guid.NewGuid(), _powerSource);
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