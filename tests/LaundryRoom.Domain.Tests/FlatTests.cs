using Xunit;

public class FlatTests
{
    [Theory]
    [InlineData("3B")]
    [InlineData("2a")]
    public void Flat_WhenCreatedWithValidLabel_ExposesItNormalised(string label)
    {
        // Arrange
        // Act
        var flat = Flat.From(label);
        var ignoreCase = true;

        // Assert
        Assert.Equal(label, flat.Label, ignoreCase);
    }

    [Theory]
    [InlineData("")]
    [InlineData("       ")]
    public void Flat_WhenCreatedWithInvalidLabel_Throws(string label)
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentException>(() => Flat.From(label));
    }

    [Theory]
    [InlineData("3B")]
    [InlineData("2a")]
    public void Flat_WhenCreatedWithSameLabel_AreEqual(string label)
    {
        // Arrange
        // Act
        var flatA = Flat.From(label);
        var flatB = Flat.From(label);

        // Assert
        Assert.Equal(flatA, flatB);
    }

    [Theory]
    [InlineData("3B")]
    [InlineData("2a")]
    public void Flat_WhenLabelsDifferCaseOnly_AreEqual(string label)
    {
        // Arrange
        // Act
        var flatLower = Flat.From(label.ToLower());
        var flatUpper = Flat.From(label.ToUpper());

        // Assert
        Assert.Equal(flatLower, flatUpper);
    }
}