using Xunit;

public class RoomTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Room_WhenCreatedWithEmptyLabel_Throws(string label)
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentException>(() => new Room(label));
    }

    [Theory]
    [InlineData("yellow")]
    [InlineData("Green")]
    public void Room_WhenCreated_HasNoPower(string label)
    {
        // Arrange
        // Act
        var room = new Room(label);

        // Assert
        Assert.False(room.IsPowerOn);
    }

    [Theory]
    [InlineData("yellow", "3b")]
    [InlineData("Green", "2A")]
    public void Room_WhenAssignedCardInserted_HasPower(string label, string owner)
    {
        // Arrange
        var room = new Room(label);
        var card = new Card(Guid.NewGuid());
        card.Assign(Flat.From(owner));

        // Act
        room.InsertCard(card);

        // Assert
        Assert.True(room.IsPowerOn);
    }

    [Theory]
    [InlineData("yellow", "3b")]
    [InlineData("Green", "2A")]
    public void Room_WhenAssignedCardInserted_ExposesOwner(string label, string owner)
    {
        // Arrange
        var room = new Room(label);
        var card = new Card(Guid.NewGuid());
        var flat = Flat.From(owner);
        card.Assign(flat);

        // Act
        room.InsertCard(card);

        // Assert
        Assert.Equal(flat, room.CardOwner);
    }

    [Theory]
    [InlineData("yellow", "3b")]
    [InlineData("Green", "2A")]
    public void Room_WhenAssignedCardEjected_HasNoPower(string label, string owner)
    {
        // Arrange
        var room = new Room(label);
        var card = new Card(Guid.NewGuid());
        card.Assign(Flat.From(owner));
        room.InsertCard(card);

        // Act
        room.EjectCard();

        // Assert
        Assert.False(room.IsPowerOn);
    }

    [Theory]
    [InlineData("yellow")]
    [InlineData("Green")]
    public void Room_WhenUnassignedCardInserted_HasNoPower(string label)
    {
        // Arrange
        var room = new Room(label);
        var card = new Card(Guid.NewGuid());

        // Act
        room.InsertCard(card);

        // Assert
        Assert.False(room.IsPowerOn);
    }

    [Theory]
    [InlineData("yellow", "2D", "3c")]
    [InlineData("Green", "1a", "1B")]
    public void Room_WhenAttemptedToInsertSecondCard_Throws(string label, string ownerA, string ownerB)
    {
        // Arrange
        var room = new Room(label);
        var cardA = new Card(Guid.NewGuid());
        var cardB = new Card(Guid.NewGuid());
        cardA.Assign(Flat.From(ownerA));
        cardB.Assign(Flat.From(ownerB));
        room.InsertCard(cardA);

        // Act
        // Assert
        Assert.Throws<InvalidOperationException>(() => room.InsertCard(cardB));
    }

    [Theory]
    [InlineData("yellow")]
    [InlineData("Green")]
    public void Room_WhenAttemptedToEjectFromEmptySlot_Throws(string label)
    {
        // Arrange
        var room = new Room(label);

        // Act
        // Assert
        Assert.Throws<InvalidOperationException>(() => room.EjectCard());
    }

    [Theory]
    [InlineData("yellow", "2D", "3c")]
    [InlineData("Green", "1a", "1B")]
    public void Room_WhenCardEjected_AcceptsNewCard(string label, string ownerA, string ownerB)
    {
        // Arrange
        var room = new Room(label);
        var cardA = new Card(Guid.NewGuid());
        var cardB = new Card(Guid.NewGuid());
        cardA.Assign(Flat.From(ownerA));
        cardB.Assign(Flat.From(ownerB));
        room.InsertCard(cardA);
        room.EjectCard();

        // Act
        room.InsertCard(cardB);

        // Assert
        Assert.True(room.IsPowerOn);
    }
}