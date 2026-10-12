public class Room
{
    public string Label { get; }
    public bool IsPowerOn => _card?.Owner != null;
    public Flat? CardOwner => _card?.Owner;

    private Card? _card;

    public Room(string label)
    {
        if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("Room label can't be null or empty.");
        Label = label.ToUpperInvariant();
    }

    public void InsertCard(Card card)
    {
        if (card == null) throw new ArgumentNullException(nameof(card));
        if (_card != null) throw new InvalidOperationException("The slot is already in use.");

        _card = card;
    }

    public void EjectCard()
    {
        if (_card == null) throw new InvalidOperationException("The slot is empty.");

        _card = null;
    }
}