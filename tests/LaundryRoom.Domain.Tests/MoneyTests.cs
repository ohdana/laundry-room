using Xunit;

public class MoneyTests
{
    private static readonly List<long> _validValues = new List<long> { 0, 1, 20, 333, 4000 };

    [Theory]
    [MemberData(nameof(GetValidValues))]
    public void Money_WhenCreatedWithNonNegativeValue_ExposesIt(long value)
    {
        // Arrange
        // Act
        var money = Money.From(value);

        // Assert
        Assert.Equal(value, money.Value);
    }

    [Theory]
    [MemberData(nameof(GetNegativeValues))]
    public void Money_WhenCreatedWithNegativeValue_Throws(long value)
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentException>(() => Money.From(value));
    }

    [Theory]
    [MemberData(nameof(GetValidValues))]
    public void Money_WhenCreatedWithSameValue_AreEqual(long value)
    {
        // Arrange
        // Act
        var money1 = Money.From(value);
        var money2 = Money.From(value);

        // Assert
        Assert.Equal(money1, money2);
    }
 
    [Theory]
    [MemberData(nameof(GetValidValueCombinations))]
    public void Money_WhenAdding_CreatesNewMoneyWithSumValue(long value1, long value2)
    {
        // Arrange
        var money1 = Money.From(value1);
        var money2 = Money.From(value2);

        // Act
        var moneySum = money1 + money2;

        // Assert
        Assert.Equal(value1 + value2, moneySum.Value);
    }

    [Theory]
    [MemberData(nameof(GetValidSubtractionCombinations))]
    public void Money_WhenSubtractingSmallerFromBigger_CreatesNewMoneyWithDiffValue(long value1, long value2)
    {
        // Arrange
        var money1 = Money.From(value1);
        var money2 = Money.From(value2);

        // Act
        var moneyDiff = money1 - money2;

        // Assert
        Assert.Equal(value1 - value2, moneyDiff.Value);
    }

    [Theory]
    [MemberData(nameof(GetInvalidSubtractionCombinations))]
    public void Money_WhenSubtractingBiggerFromSmaller_Throws(long value1, long value2)
    {
        // Arrange
        var money1 = Money.From(value1);
        var money2 = Money.From(value2);

        // Act
        // Assert
        Assert.Throws<ArgumentException>(() => money1 - money2);
    }
    
    [Theory]
    [MemberData(nameof(GetValidValueCombinations))]
    public void Money_WhenComparingTwo_ReturnsSameAsComparingValues(long value1, long value2)
    {
        // Arrange
        var money1 = Money.From(value1);
        var money2 = Money.From(value2);
        
        // Act
        var isLess = money1 < money2;
        var isGreater = money1 > money2;
        var isLessOrEqual = money1 <= money2;
        var isGreaterOrEqual = money1 >= money2;

        // Assert
        Assert.Equal(value1 < value2, isLess);
        Assert.Equal(value1 > value2, isGreater);
        Assert.Equal(value1 <= value2, isLessOrEqual);
        Assert.Equal(value1 >= value2, isGreaterOrEqual);
    }

    public static IEnumerable<object[]> GetInvalidSubtractionCombinations()
    {
        for (int i = 0; i < _validValues.Count; i++)
        {
            for (int j = i + 1; j < _validValues.Count; j++)
            {
                yield return new object[] { _validValues[i], _validValues[j] };
            }
        }
    }

    public static IEnumerable<object[]> GetValidSubtractionCombinations()
    {
        for (int i = 0; i < _validValues.Count; i++)
        {
            for (int j = i; j < _validValues.Count; j++)
            {
                yield return new object[] { _validValues[j], _validValues[i] };
            }
        }
    }

    public static IEnumerable<object[]> GetValidValueCombinations()
    {
        foreach (var value1 in _validValues)
        {
            foreach (var value2 in _validValues)
            {
                yield return new object[] { value1, value2 };
            }
        }
    }

    public static IEnumerable<object[]> GetNegativeValues()
    {
        foreach (var value in _validValues)
        {
            if (value == 0) continue;
            yield return new object[] { value * (-1) };
        }
    }

    public static IEnumerable<object[]> GetValidValues()
    {
        foreach (var value in _validValues)
        {
            yield return new object[] { value };
        }
    }
}