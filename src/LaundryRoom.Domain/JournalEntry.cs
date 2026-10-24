public abstract record JournalEntry
{
    private JournalEntry() { }
    public sealed record Washer(Guid WasherId, WasherProgram Program, CycleOutcome Outcome, Kwh electricityConsumed): JournalEntry;
    public sealed record Dryer(Guid DryerId, DryerProgram Program, CycleOutcome Outcome): JournalEntry;
}