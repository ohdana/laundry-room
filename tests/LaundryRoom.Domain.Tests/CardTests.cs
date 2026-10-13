using Xunit;

public class CardTests
{
    [Fact]
    public void Card_WhenCreatedWithValidId_HasNoOwner()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var card = new Card(id);

        // Assert
        Assert.Null(card.Owner);
    }

    [Fact]
    public void Card_WhenCreatedWithInvalidId_Throws()
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentException>(() => new Card(Guid.Empty));
    }

    [Theory]
    [InlineData("3B")]
    [InlineData("2a")]
    public void Card_WhenAssigned_HasOwner(string owner)
    {
        // Arrange
        var card = new Card(Guid.NewGuid());
        var flat = Flat.From(owner);

        // Act
        card.Assign(flat);

        // Assert
        Assert.NotNull(card.Owner);
        Assert.Equal(flat, card.Owner);
    }

    [Fact]
    public void Card_WhenAssignedNull_Throws()
    {
        // Arrange
        var card = new Card(Guid.NewGuid());

        // Act
        // Assert
        Assert.Throws<ArgumentNullException>(() => card.Assign(null!));
    }

    [Theory]
    [InlineData("3B", "5c")]
    [InlineData("2a", "1a")]
    public void Card_WhenHasOwnerAndAssignedAgain_Throws(string oldOwner, string newOwner)
    {
        // Arrange
        var card = new Card(Guid.NewGuid());
        card.Assign(Flat.From(oldOwner));

        // Act
        // Assert
        Assert.Throws<InvalidOperationException>(() => card.Assign(Flat.From(newOwner)));
    }

    [Theory]
    [InlineData("3B")]
    [InlineData("2a")]
    public void Card_WhenReset_HasNoOwner(string owner)
    {
        // Arrange
        var card = new Card(Guid.NewGuid());
        card.Assign(Flat.From(owner));

        // Act
        card.Reset();

        // Assert
        Assert.Null(card.Owner);
    }
}