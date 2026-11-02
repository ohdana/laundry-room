using Xunit;

public class AllocatorTests
{
    [Fact]
    public void Allocator_WhenTotalDividesEvenly_GivesEqualShares()
    {
        // Arrange
        var total = Money.From(90);
        var weights = GetKwhReadOnlyList(1, 1, 1);
        Money[] expected = { Money.From(30), Money.From(30), Money.From(30) };

        // Act
        var actual = Allocator.Allocate(total, weights);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Allocator_WhenOneFrancSplitThreeWays_Gives34_33_33()
    {
        // Arrange
        var total = Money.From(100);
        var weights = GetKwhReadOnlyList(1, 1, 1);
        Money[] expected = { Money.From(34), Money.From(33), Money.From(33) };

        // Act
        var actual = Allocator.Allocate(total, weights);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Allocator_WhenFiveRappenSplit70_30_Gives4_1()
    {
        // Arrange
        var total = Money.From(5);
        var weights = GetKwhReadOnlyList(7, 3);
        Money[] expected = { Money.From(4), Money.From(1) };

        // Act
        var actual = Allocator.Allocate(total, weights);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Allocator_WhenLeftoverRemainderDiffer_GivesLeftoverToLargestRemainder()
    {
        // Arrange
        var total = Money.From(324);
        var weights = GetKwhReadOnlyList(5.3m, 6.7m);
        Money[] expected = { Money.From(143), Money.From(181) };

        // Act
        var actual = Allocator.Allocate(total, weights);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Allocator_WhenWeightIsZero_ShareIsZero()
    {
        // Arrange
        var total = Money.From(10);
        var weights = GetKwhReadOnlyList(2, 0, 2);
        Money[] expected = { Money.From(5), Money.From(0), Money.From(5) };

        // Act
        var actual = Allocator.Allocate(total, weights);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Allocator_WhenTotalIsZero_AllSharesZero()
    {
        // Arrange
        var total = Money.From(0);
        var weights = GetKwhReadOnlyList(3, 5);
        Money[] expected = { Money.From(0), Money.From(0) };

        // Act
        var actual = Allocator.Allocate(total, weights);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Allocator_WhenSingleWeight_GetsEverything()
    {
        // Arrange
        var total = Money.From(77);
        var weights = GetKwhReadOnlyList(4);
        Money[] expected = { Money.From(77) };

        // Act
        var actual = Allocator.Allocate(total, weights);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Allocator_WhenAllWeightsZero_Throws()
    {
        // Arrange
        var total = Money.From(10);
        var weights = GetKwhReadOnlyList(0, 0);

        // Act
        // Assert
        Assert.Throws<ArgumentException>(() => Allocator.Allocate(total, weights));
    }

    [Fact]
    public void Allocator_WhenRemaindersTie_GivesLeftoverToFirst()
    {
        // Arrange
        var total = Money.From(1);
        var weights = GetKwhReadOnlyList(1, 1);
        Money[] expected = { Money.From(1), Money.From(0) };

        // Act
        var actual = Allocator.Allocate(total, weights);

        // Assert
        Assert.Equal(expected, actual);
    }

    private static IReadOnlyList<Kwh> GetKwhReadOnlyList(params decimal[] nums)
    {
        var list = new List<Kwh>();
        foreach (var num in nums)
            list.Add(Kwh.From(num));
        return list;
    }
}