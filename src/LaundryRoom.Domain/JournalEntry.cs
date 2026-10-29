public abstract record JournalEntry
{
    private JournalEntry() { }
    public sealed record Washer(Guid WasherId, string RoomLabel, Flat CardOwner, WasherProgram Program, CycleOutcome Outcome, Kwh ElectricityConsumed) : JournalEntry;
    public sealed record Dryer(Guid DryerId, string RoomLabel, Flat CardOwner, DryerProgram Program, CycleOutcome Outcome, Kwh ElectricityConsumed) : JournalEntry;
    public sealed record Fan(Guid fanId, string RoomLabel, Flat CardOwner, FanProgram Program, DateTimeOffset StartedAt, DateTimeOffset EndedAt, Kwh ElectricityConsumed) : JournalEntry;
}