using Xunit;
using NSubstitute;

public class DryerTests
{
    private IPowerSource _powerSource;
    public DryerTests() => _powerSource = Substitute.For<IPowerSource>();

    [Fact]
    public void Dryer_WhenCreatedWithValidId_IsOff()
    {
        // Arrange
        // Act
        var dryer = new Dryer(Guid.NewGuid(), _powerSource);

        // Assert
        Assert.False(dryer.IsOn);
    }

    [Fact]
    public void Dryer_WhenCreatedWithValidId_IsIdle()
    {
        // Arrange
        // Act
        var dryer = new Dryer(Guid.NewGuid(), _powerSource);

        // Assert
        Assert.IsType<DryerState.Idle>(dryer.State);
    }

    [Fact]
    public void Dryer_WhenCreatedWithInalidId_Throws()
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentException>(() => new Dryer(Guid.Empty, _powerSource));
    }

    [Fact]
    public void Dryer_WhenCreatedWithNoPowerSource_Throws()
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentNullException>(() => new Dryer(Guid.NewGuid(), null!));
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
        var dryer = new Dryer(Guid.NewGuid(), _powerSource);

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

    private Dryer GetPoweredDryer()
    {
        var dryer = new Dryer(Guid.NewGuid(), _powerSource);
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
}