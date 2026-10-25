public abstract record JournalEntry
{
    private JournalEntry() { }
    public sealed record Washer(Guid WasherId, string RoomLabel, Flat CardOwner, WasherProgram Program, CycleOutcome Outcome, Kwh ElectricityConsumed): JournalEntry;
    public sealed record Dryer(Guid DryerId, DryerProgram Program, CycleOutcome Outcome, Kwh ElectricityConsumed): JournalEntry;
}