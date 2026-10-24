using Xunit;

public class MeterReadingTests
{
    private static readonly List<decimal> _values = new List<decimal> { 0, 0.01m, 0.2m, 3, 40, 500, 6000 };

    [Theory]
    [MemberData(nameof(GetValues))]
    public void MeterReading_WhenCreated_ExposesValue(Kwh value)
    {
        // Arrange
        // Act
        var reading = MeterReading.From(value);

        // Assert
        Assert.Equal(value, reading.Kwh);
    }

    [Theory]
    [MemberData(nameof(GetValues))]
    public void MeterReading_WhenCreatedWithSameValue_AreEqual(Kwh value)
    {
        // Arrange
        // Act
        var reading1 = MeterReading.From(value);
        var reading2 = MeterReading.From(value);

        // Assert
        Assert.Equal(reading1, reading2);
    }

    public static IEnumerable<object[]> GetValues()
    {
        foreach (var value in _values)
        {
            yield return new object[] { Kwh.From(value) };
        }
    }
}