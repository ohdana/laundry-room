using Xunit;
using NSubstitute;

public class FanTests
{
    private IPowerSource _powerSource;
    public FanTests() => _powerSource = Substitute.For<IPowerSource>();

    [Fact]
    public void Fan_WhenCreatedWithValidId_IsOff()
    {
        // Arrange
        // Act
        var fan = new Fan(Guid.NewGuid(), _powerSource);

        // Assert
        Assert.False(fan.IsOn);
    }

    [Fact]
    public void Fan_WhenCreatedWithInalidId_Throws()
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentException>(() => new Fan(Guid.Empty, _powerSource));
    }

    [Fact]
    public void Fan_WhenCreatedWithNoPowerSource_Throws()
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentNullException>(() => new Fan(Guid.NewGuid(), null!));
    }

    [Fact]
    public void Fan_WhenPowered_IsOn()
    {
        // Arrange
        // Act
        var fan = GetPoweredFan();

        // Assert
        Assert.True(fan.IsOn);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Fan_WhenOnAndStartCalled_IsRunning(FanProgram program)
    {
        // Arrange
        var fan = GetPoweredFan();
        fan.SelectProgram(program);

        // Act
        fan.Start();

        // Assert
        var running = Assert.IsType<FanState.Running>(fan.State);
        Assert.Equal(program, running.Program);
    }

    [Fact]
    public void Fan_WhenOffAndStartCalled_Throws()
    {
        // Arrange
        var fan = new Fan(Guid.NewGuid(), _powerSource);
        SwitchPowerOff();

        // Act
        // Assert
        Assert.Throws<InvalidOperationException>(() => fan.Start());
    }

    [Fact]
    public void Fan_WhenPowerLostAndStartCalled_Throws()
    {
        // Arrange
        var fan = GetPoweredFan();

        // Act
        SwitchPowerOff();

        // Assert
        Assert.Throws<InvalidOperationException>(() => fan.Start());
    }
    
    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Fan_WhenRunningAndStartCalled_Throws(FanProgram program)
    {
        // Arrange
        var fan = GetPoweredFan();
        fan.SelectProgram(program);
        fan.Start();

        // Act
        // Assert
        Assert.Throws<InvalidOperationException>(() => fan.Start());
    }

    [Fact]
    public void Fan_WhenOnAndStopCalled_IsIdle()
    {
        // Arrange
        var fan = GetPoweredFan();

        // Act
        fan.Stop();

        // Assert
        Assert.IsType<FanState.Idle>(fan.State);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Fan_WhenRunningAndStopCalled_IsIdle(FanProgram program)
    {

        // Arrange
        var fan = GetPoweredFan();
        fan.SelectProgram(program);
        fan.Start();

        // Act
        fan.Stop();

        // Assert
        Assert.IsType<FanState.Idle>(fan.State);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Fan_WhenRunningAndPowerLost_IsOff(FanProgram program)
    {

        // Arrange
        var fan = GetPoweredFan();
        fan.SelectProgram(program);
        fan.Start();

        // Act
        SwitchPowerOff();

        // Assert
        Assert.False(fan.IsOn);
    }

    [Fact]
    public void Fan_WhenIdleAndPowerLostAndBackOn_IsIdle()
    {
        // Arrange
        var fan = GetPoweredFan();
        fan.Start();
        fan.Stop();
        SwitchPowerOff();

        // Act
        SwitchPowerOn();

        // Assert
        Assert.IsType<FanState.Idle>(fan.State);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Fan_WhenRunningAndPowerLostAndBackOn_IsRunning(FanProgram program)
    {
        // Arrange
        var fan = GetPoweredFan();
        fan.SelectProgram(program);
        fan.Start();
        SwitchPowerOff();

        // Act
        SwitchPowerOn();

        // Assert
        Assert.True(fan.IsOn);
        var running = Assert.IsType<FanState.Running>(fan.State);
        Assert.Equal(program, running.Program);
    }

    private Fan GetPoweredFan()
    {
        var fan = new Fan(Guid.NewGuid(), _powerSource);
        SwitchPowerOn();

        return fan;
    }

    private void SwitchPowerOn()
    {
        _powerSource.IsPowerOn.Returns(true);
        _powerSource.PowerOn += Raise.Event<Action>();
    }

    private void SwitchPowerOff()
    {
        _powerSource.IsPowerOn.Returns(false);
        _powerSource.PowerOff += Raise.Event<Action>();
    }

    public static IEnumerable<object[]> GetPrograms()
    {
        var programs = Enum.GetValues<FanProgram>();
        for (int i = 0; i < programs.Length; i++)
        {
            yield return new object[] { programs[i] };
        }
    }
}