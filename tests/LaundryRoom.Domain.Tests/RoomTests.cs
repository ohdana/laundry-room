using Xunit;

public class RoomTests
{
    private static readonly string[] _validRoomLabels = { "yellow", "Green" };
    private static readonly string[] _validFlatLabels = { "1a", "2B" };

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
    [MemberData(nameof(GetRoomLabels))]
    public void Room_WhenCreated_HasNoPower(string label)
    {
        // Arrange
        // Act
        var room = new Room(label);

        // Assert
        Assert.False(room.IsPowerOn);
    }

    [Theory]
    [MemberData(nameof(GetRoomFlatCombinations))]
    public void Room_WhenAssignedCardInserted_HasPower(string label, string owner)
    {
        // Arrange
        var room = new Room(label);
        var card = GetAssignedCard(owner);

        // Act
        room.InsertCard(card);

        // Assert
        Assert.True(room.IsPowerOn);
    }

    [Theory]
    [MemberData(nameof(GetRoomFlatCombinations))]
    public void Room_WhenAssignedCardInserted_ExposesOwner(string label, string owner)
    {
        // Arrange
        var room = new Room(label);
        var card = GetAssignedCard(owner);

        // Act
        room.InsertCard(card);

        // Assert
        Assert.Equal(Flat.From(owner), room.CardOwner);
    }

    [Theory]
    [MemberData(nameof(GetRoomFlatCombinations))]
    public void Room_WhenAssignedCardEjected_HasNoPower(string label, string owner)
    {
        // Arrange
        var room = new Room(label);
        var card = GetAssignedCard(owner);
        room.InsertCard(card);

        // Act
        room.EjectCard();

        // Assert
        Assert.False(room.IsPowerOn);
    }

    [Theory]
    [MemberData(nameof(GetRoomLabels))]
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
    [MemberData(nameof(GetRoomFlatFlatCombinations))]
    public void Room_WhenAttemptedToInsertSecondCard_Throws(string label, string ownerA, string ownerB)
    {
        // Arrange
        var room = new Room(label);
        var cardA = GetAssignedCard(ownerA);
        var cardB = GetAssignedCard(ownerB);
        room.InsertCard(cardA);

        // Act
        // Assert
        Assert.Throws<InvalidOperationException>(() => room.InsertCard(cardB));
    }

    [Theory]
    [MemberData(nameof(GetRoomLabels))]
    public void Room_WhenAttemptedToEjectFromEmptySlot_Throws(string label)
    {
        // Arrange
        var room = new Room(label);

        // Act
        // Assert
        Assert.Throws<InvalidOperationException>(() => room.EjectCard());
    }

    [Theory]
    [MemberData(nameof(GetRoomFlatFlatCombinations))]
    public void Room_WhenCardEjected_AcceptsNewCard(string label, string ownerA, string ownerB)
    {
        // Arrange
        var room = new Room(label);
        var cardA = GetAssignedCard(ownerA);
        room.InsertCard(cardA);
        room.EjectCard();
        
        var cardB = GetAssignedCard(ownerB);

        // Act
        room.InsertCard(cardB);

        // Assert
        Assert.True(room.IsPowerOn);
    }

    private Card GetAssignedCard(string owner)
    {
        var card = new Card(Guid.NewGuid());
        card.Assign(Flat.From(owner));

        return card;
    }

    public static IEnumerable<object[]> GetRoomLabels()
    {
        for (int i = 0; i < _validRoomLabels.Length; i++)
        {
            yield return new object[] { _validRoomLabels[i] };
        }
    }

    public static IEnumerable<object[]> GetRoomFlatCombinations()
    {
        for (int i = 0; i < _validRoomLabels.Length; i++)
        {
            for (int j = 0; j < _validFlatLabels.Length; j++)
            {
                yield return new object[] { _validRoomLabels[i], _validFlatLabels[j] };
            }
        }
    }

    public static IEnumerable<object[]> GetRoomFlatFlatCombinations()
    {
        for (int i = 0; i < _validRoomLabels.Length; i++)
        {
            for (int j = 0; j < _validFlatLabels.Length; j++)
            {
                for (int k = 0; k < _validFlatLabels.Length; k++)
                {
                    yield return new object[] { _validRoomLabels[i], _validFlatLabels[j], _validFlatLabels[k] };
                }
            }
        }
    }
}