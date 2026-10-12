public sealed class Card
{
    public Guid Id { get; }
    public Flat Owner { get; private set; }

    public Card(Guid id)
    {
        if (id == Guid.Empty) throw new ArgumentException("Card id can't be empty.", nameof(id));
        Id = id;
    }
    
    public void Assign(Flat owner)
    {
        if (owner == null) throw new ArgumentNullException(nameof(owner));
        if (Owner != null) throw new InvalidOperationException("Card is already assigned. To assign a new owner, call Reset() first.");
        Owner = owner;
    }

    public void Reset() => Owner = null;
}