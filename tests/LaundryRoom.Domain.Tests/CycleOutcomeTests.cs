using Xunit;
using NSubstitute;

public class CycleOutcomeTests
{
    [Fact]
    public void CycleOutcome_WhenCompleted_ReportsStartedAtCompletedAt()
    {
        // Arrange
        var completedAt = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var startedAt = completedAt.AddMinutes(-30);

        // Act
        var cycleOutcome = new CycleOutcome.Completed(startedAt, completedAt);

        // Assert
        Assert.Equal(startedAt, cycleOutcome.StartedAt);
        Assert.Equal(completedAt, cycleOutcome.CompletedAt);
    }

    [Fact]
    public void CycleOutcome_WhenInterrupted_ReportsStartedAtInterruptedAt()
    {
        // Arrange
        var interruptedAt = new DateTimeOffset(2026, 2, 4, 11, 0, 0, TimeSpan.Zero);
        var startedAt = interruptedAt.AddMinutes(-5);

        // Act
        var cycleOutcome = new CycleOutcome.Interrupted(startedAt, interruptedAt);

        // Assert
        Assert.Equal(startedAt, cycleOutcome.StartedAt);
        Assert.Equal(interruptedAt, cycleOutcome.InterruptedAt);
    }
   
    [Fact] 
    public void CycleOutcome_WhenCompleted_RejectsCompletedAtBeforeStartedAt()
    {
        // Arrange
        var completedAt = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var startedAt = completedAt.AddMinutes(2);

        // Act
        // Assert
        Assert.Throws<ArgumentException>(() => new CycleOutcome.Completed(startedAt, completedAt));
    }
    
    [Fact]
    public void CycleOutcome_WhenInterrupted_RejectsInterruptedAtBeforeStartedAt()
    {
        // Arrange
        var interruptedAt = new DateTimeOffset(2026, 2, 4, 11, 0, 0, TimeSpan.Zero);
        var startedAt = interruptedAt.AddMinutes(3);

        // Act
        // Assert
        Assert.Throws<ArgumentException>(() => new CycleOutcome.Interrupted(startedAt, interruptedAt));
    }
    
    [Fact]
    public void CycleOutcome_WhenInterrupted_AllowsInterruptedSameAsStartedAt()
    {
        // Arrange
        var interruptedAt = new DateTimeOffset(2026, 2, 4, 11, 0, 0, TimeSpan.Zero);
        var startedAt = interruptedAt;

        // Act
        var cycleOutcome = new CycleOutcome.Interrupted(startedAt, interruptedAt);

        // Assert
        Assert.Equal(startedAt, cycleOutcome.StartedAt);
        Assert.Equal(interruptedAt, cycleOutcome.InterruptedAt);
    }
}