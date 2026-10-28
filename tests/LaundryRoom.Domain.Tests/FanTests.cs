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
    public void Fan_WhenCreatedWithInvalidId_Throws()
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
    public void Fan_WhenPoweredAndSwitchedOn_IsOn()
    {
        // Arrange
        var fan = GetPoweredFan();

        // Act
        fan.SwitchOn();

        // Assert
        Assert.True(fan.IsOn);
    }

    [Fact]
    public void Fan_WhenPoweredAndNotSwitchedOn_IsOff()
    {
        // Arrange
        // Act
        var fan = GetPoweredFan();

        // Assert
        Assert.False(fan.IsOn);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Fan_WhenSwitchedOn_IsRunning(FanProgram program)
    {
        // Arrange
        var fan = GetPoweredFan();

        // Act
        fan.SwitchOn();
        fan.SelectProgram(program);

        // Assert
        var running = Assert.IsType<FanState.Running>(fan.State);
        Assert.Equal(program, running.Program);
    }

    [Fact]
    public void Fan_WhenOnAndSwitchOffCalled_IsOff()
    {
        // Arrange
        var fan = GetPoweredFan();
        fan.SwitchOn();

        // Act
        fan.SwitchOff();

        // Assert
        Assert.IsType<FanState.Off>(fan.State);
    }

    [Theory]
    [MemberData(nameof(GetProgramCombinations))]
    public void Fan_WhenRunningAndNewProgramSelected_RunsNewProgram(FanProgram program1, FanProgram program2)
    {
        // Arrange
        var fan = GetPoweredFan();
        fan.SwitchOn();
        fan.SelectProgram(program1);

        // Act
        fan.SelectProgram(program2);

        // Assert
        var running = Assert.IsType<FanState.Running>(fan.State);
        Assert.Equal(program2, running.Program);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Fan_WhenRunningAndSwitchOffCalled_IsOff(FanProgram program)
    {
        // Arrange
        var fan = GetPoweredFan();
        fan.SwitchOn();
        fan.SelectProgram(program);

        // Act
        fan.SwitchOff();

        // Assert
        Assert.IsType<FanState.Off>(fan.State);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Fan_WhenRunningAndPowerLost_IsOff(FanProgram program)
    {
        // Arrange
        var fan = GetPoweredFan();
        fan.SwitchOn();
        fan.SelectProgram(program);

        // Act
        SwitchPowerOff();

        // Assert
        Assert.False(fan.IsOn);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Fan_WhenRunningAndPowerLostAndBackOn_IsRunning(FanProgram program)
    {
        // Arrange
        var fan = GetPoweredFan();
        fan.SwitchOn();
        fan.SelectProgram(program);
        SwitchPowerOff();

        // Act
        SwitchPowerOn();

        // Assert
        Assert.True(fan.IsOn);
        var running = Assert.IsType<FanState.Running>(fan.State);
        Assert.Equal(program, running.Program);
    }

    [Fact]
    public void Fan_WhenSwitchedOffThenPowerLostAndBackOn_IsOff()
    {
        // Arrange
        var fan = GetPoweredFan();
        fan.SwitchOn();
        fan.SwitchOff();

        // Act
        SwitchPowerOff();
        SwitchPowerOn();

        // Assert
        Assert.False(fan.IsOn);
    }

    [Fact]
    public void Fan_WhenPoweredAndSwitchOnCalled_ReportsStarted()
    {
        // Arrange
        var started = new List<FanProgram>();
        var fan = GetPoweredFan();
        fan.Started += p => started.Add(p);

        // Act
        fan.SwitchOn();
        
        // Assert
        Assert.Single(started);
    }

    [Fact]
    public void Fan_WhenNotPoweredAndSwitchOnCalled_DoesNotReportStarted()
    {
        // Arrange
        var started = new List<FanProgram>();
        var fan = GetPoweredFan();
        fan.Started += p => started.Add(p);
        SwitchPowerOff();

        // Act
        fan.SwitchOn();
        
        // Assert
        Assert.Empty(started);
    }

    [Fact]
    public void Fan_WhenOnAndSwitchOnCalled_DoesNotReportStarted()
    {
        // Arrange
        var started = new List<FanProgram>();
        var fan = GetPoweredFan();
        fan.SwitchOn();
        fan.Started += p => started.Add(p);

        // Act
        fan.SwitchOn();
        
        // Assert
        Assert.Empty(started);
    }

    [Fact]
    public void Fan_WhenOnAndSwitchOffCalled_ReportsStopped()
    {
        var stopped = new List<FanProgram>();
        var fan = GetPoweredFan();
        fan.Stopped += p => stopped.Add(p);
        fan.SwitchOn();

        // Act
        fan.SwitchOff();
        
        // Assert
        Assert.Single(stopped);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Fan_WhenRunningAndPowerLost_ReportsStopped(FanProgram program)
    {
        // Arrange
        var stopped = new List<FanProgram>();
        var fan = GetPoweredFan();
        fan.SwitchOn();
        fan.SelectProgram(program);
        fan.Stopped += p => stopped.Add(p);

        // Act
        SwitchPowerOff();

        // Assert
        Assert.Single(stopped);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Fan_WhenRunningAndPowerLostAndBackOn_ReportsStarted(FanProgram program)
    {
        // Arrange
        var started = new List<FanProgram>();
        var fan = GetPoweredFan();
        fan.SwitchOn();
        fan.SelectProgram(program);
        fan.Started += p => started.Add(p);

        // Act
        SwitchPowerOff();
        SwitchPowerOn();

        // Assert
        Assert.Equal(program, Assert.Single(started));
    }

    [Fact]
    public void Fan_WhenSwitchedOffThenPowerLostAndBackOn_DoesNotReportStartedOrStopped()
    {
        // Arrange
        var stopped = new List<FanProgram>();
        var started = new List<FanProgram>();
        var fan = GetPoweredFan();
        fan.SwitchOn();
        fan.SwitchOff();
        fan.Started += p => started.Add(p);
        fan.Stopped += p => stopped.Add(p);

        // Act
        SwitchPowerOff();
        SwitchPowerOn();

        // Assert
        Assert.Empty(started);
        Assert.Empty(stopped);
    }

    [Fact]
    public void Fan_WhenOffAndSelectedNewProgram_DoesNotReportStartedOrStopped()
    {
        // Arrange
        var stopped = new List<FanProgram>();
        var started = new List<FanProgram>();
        var fan = GetPoweredFan();
        fan.Started += p => started.Add(p);
        fan.Stopped += p => stopped.Add(p);

        // Act
        fan.SwitchOff();

        // Assert
        Assert.Empty(started);
        Assert.Empty(stopped);
    }

    [Theory]
    [MemberData(nameof(GetPrograms))]
    public void Fan_WhenRunningAndSelectedSameProgram_DoesNotReportStartedOrStopped(FanProgram program)
    {
        // Arrange
        var stopped = new List<FanProgram>();
        var started = new List<FanProgram>();
        var fan = GetPoweredFan();
        fan.SwitchOn();
        fan.SelectProgram(program);
        fan.Started += p => started.Add(p);
        fan.Stopped += p => stopped.Add(p);

        // Act
        fan.SelectProgram(program);

        // Assert
        Assert.Empty(started);
        Assert.Empty(stopped);
    }

    [Theory]
    [MemberData(nameof(GetProgramCombinations))]
    public void Fan_WhenRunningAndSelectedNewProgram_ReportStoppedOldStartedNew(FanProgram program1, FanProgram program2)
    {
        // Arrange
        var stopped = new List<FanProgram>();
        var started = new List<FanProgram>();
        var fan = GetPoweredFan();
        fan.SwitchOn();
        fan.SelectProgram(program1);
        fan.Started += p => started.Add(p);
        fan.Stopped += p => stopped.Add(p);

        // Act
        fan.SelectProgram(program2);

        // Assert
        Assert.Equal(program1, Assert.Single(stopped));
        Assert.Equal(program2, Assert.Single(started));
    }

    private Fan GetPoweredFan()
    {
        SwitchPowerOn();
        var fan = new Fan(Guid.NewGuid(), _powerSource);

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

    public static IEnumerable<object[]> GetProgramCombinations()
    {
        var programs = Enum.GetValues<FanProgram>();
        foreach (var program1 in programs)
            foreach (var program2 in programs)
            {
                if (program1 == program2) continue;
                yield return new object[] { program1, program2 };
            }
    }
}