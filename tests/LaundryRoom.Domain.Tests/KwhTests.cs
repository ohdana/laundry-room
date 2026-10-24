using Xunit;

public class KwhTests
{
    private static readonly List<decimal> _validValues = new List<decimal> { 0, 0.01m, 0.2m, 3, 40, 500, 6000 };

    [Theory]
    [MemberData(nameof(GetValidValues))]
    public void Kwh_WhenCreatedWithNonNegativeValue_ExposesIt(decimal value)
    {
        // Arrange
        // Act
        var kwh = Kwh.From(value);

        // Assert
        Assert.Equal(value, kwh.Value);
    }

    [Theory]
    [MemberData(nameof(GetNegativeValues))]
    public void Kwh_WhenCreatedWithNegativeValue_Throws(decimal value)
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentException>(() => Kwh.From(value));
    }

    [Theory]
    [MemberData(nameof(GetValidValues))]
    public void Kwh_WhenCreatedWithSameValue_AreEqual(decimal value)
    {
        // Arrange
        // Act
        var kwh1 = Kwh.From(value);
        var kwh2 = Kwh.From(value);

        // Assert
        Assert.Equal(kwh1, kwh2);
    }
 
    [Theory]
    [MemberData(nameof(GetValidValueCombinations))]
    public void Kwh_WhenAdding_CreatesNewKwhWithSumValue(decimal value1, decimal value2)
    {
        // Arrange
        var kwh1 = Kwh.From(value1);
        var kwh2 = Kwh.From(value2);

        // Act
        var kwhSum = kwh1 + kwh2;

        // Assert
        Assert.Equal(value1 + value2, kwhSum.Value);
    }

    [Theory]
    [MemberData(nameof(GetValidSubtractionCombinations))]
    public void Kwh_WhenSubtracting_CreatesNewKwhWithDiffValue(decimal value1, decimal value2)
    {
        // Arrange
        var kwh1 = Kwh.From(value1);
        var kwh2 = Kwh.From(value2);

        // Act
        var kwhDiff = kwh1 - kwh2;

        // Assert
        Assert.Equal(value1 - value2, kwhDiff.Value);
    }
    
    [Theory]
    [MemberData(nameof(GetValidValueCombinations))]
    public void Kwh_WhenComparingTwo_ReturnsSameAsComparingValues(decimal value1, decimal value2)
    {
        // Arrange
        var kwh1 = Kwh.From(value1);
        var kwh2 = Kwh.From(value2);
        
        // Act
        var isLess = kwh1 < kwh2;
        var isGreater = kwh1 > kwh2;
        var isLessOrEqual = kwh1 <= kwh2;
        var isGreaterOrEqual = kwh1 >= kwh2;

        // Assert
        Assert.Equal(value1 < value2, isLess);
        Assert.Equal(value1 > value2, isGreater);
        Assert.Equal(value1 <= value2, isLessOrEqual);
        Assert.Equal(value1 >= value2, isGreaterOrEqual);
    }

    public static IEnumerable<object[]> GetValidSubtractionCombinations()
    {
        for (int i = 0; i < _validValues.Count; i++)
        {
            for (int j = i + 1; j < _validValues.Count; j++)
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