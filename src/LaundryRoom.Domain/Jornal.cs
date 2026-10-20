public class Journal
{
    public IReadOnlyList<JournalEntry> Entries => _entries.AsReadOnly();

    private readonly TimeProvider _clock;
    private readonly List<JournalEntry> _entries = new();

    public Journal(TimeProvider clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public void Attach(Washer washer)
    {
        DateTimeOffset startedAt = default;

        washer.ProgramStarted      += OnStarted;
        washer.ProgramCompleted    += OnCompleted;
        washer.ProgramInterrupted  += OnInterrupted;

        void OnStarted(WasherProgram p)     => startedAt = _clock.GetUtcNow();

        void OnCompleted(WasherProgram p)   => 
            Add(washer.Id, p, new CycleOutcome.Completed(startedAt, _clock.GetUtcNow()));

        void OnInterrupted(WasherProgram p) => 
            Add(washer.Id, p, new CycleOutcome.Interrupted(startedAt, _clock.GetUtcNow()));
    }

    private void Add(Guid washerId, WasherProgram program, CycleOutcome outcome)
    {
        var entry = new JournalEntry.Washer(washerId, program, outcome);
        _entries.Add(entry);
    }
}