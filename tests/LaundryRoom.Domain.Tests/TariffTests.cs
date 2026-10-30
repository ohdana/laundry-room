using Xunit;

public class TariffTests
{
    private static readonly List<long> _validValues = new List<long> { 0, 1, 20, 333, 4000 };

    [Theory]
    [MemberData(nameof(GetValidValues))]
    public void Tariff_WhenCreatedWithNonNegativeValue_ExposesIt(Money value)
    {
        // Arrange
        // Act
        var tariff = Tariff.From(value);

        // Assert
        Assert.Equal(value, tariff.PricePerKwh);
    }

    [Theory]
    [MemberData(nameof(GetValidValues))]
    public void Tariff_WhenCreatedWithSameValue_AreEqual(Money value)
    {
        // Arrange
        // Act
        var tariff1 = Tariff.From(value);
        var tariff2 = Tariff.From(value);

        // Assert
        Assert.Equal(tariff1, tariff2);
    }

    [Fact]
    public void Tariff_WhenCostCalledWithZeroKwh_ReturnsZero()
    {
        // Arrange
        var value = Money.From(100);
        var tariff = Tariff.From(value);

        // Act
        var cost = tariff.Cost(Kwh.From(0));

        // Assert
        Assert.Equal(0, cost.Value);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(4.2)]
    [InlineData(4.5)]
    [InlineData(0.02)]
    [InlineData(0.5)]
    public void Tariff_WhenCostCalledWithZeroPrice_ReturnsZero(double kwh)
    {
        // Arrange
        var tariff = Tariff.From(Money.From(0));

        // Act
        var cost = tariff.Cost(Kwh.From((decimal)kwh));

        // Assert
        Assert.Equal(0, cost.Value);
    }

    [Theory]
    [InlineData(27, 3, 81)]
    [InlineData(27, 4.2, 113)]
    [InlineData(27, 4.5, 122)]
    [InlineData(27, 0.02, 1)]
    [InlineData(1, 0.5, 1)]
    public void Tariff_WhenCostCalledWithNonZero_ReturnsRoundedPriceKwhProduct(long price, double kwh, long expectedCost)
    {
        // Arrange
        var tariff = Tariff.From(Money.From(price));

        // Act
        var actualCost = tariff.Cost(Kwh.From((decimal)kwh));

        // Assert
        Assert.Equal(Money.From(expectedCost), actualCost);
    }

    public static IEnumerable<object[]> GetValidValues()
    {
        foreach (var value in _validValues)
            yield return new object[] { Money.From(value) };
    }
}