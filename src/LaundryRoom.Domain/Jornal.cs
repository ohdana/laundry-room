public class Journal
{
    public IReadOnlyList<JournalEntry> Entries => _entries.AsReadOnly();

    private readonly TimeProvider _clock;
    private readonly List<JournalEntry> _entries = new();

    public Journal(TimeProvider clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public void Attach(Washer washer, IElectricityMeter meter)
    {
        DateTimeOffset startedAt = default;
        MeterReading? readingBefore = null;

        washer.ProgramStarted      += OnStarted;
        washer.ProgramCompleted    += OnCompleted;
        washer.ProgramInterrupted  += OnInterrupted;

        void OnStarted(WasherProgram p)
        {
            startedAt = _clock.GetUtcNow();
            readingBefore = meter.Read();
        }

        void OnCompleted(WasherProgram p)
        {
            var electricityConsumed = CalculateElectricityConsumed(readingBefore, meter.Read());
            Add(washer.Id, p, new CycleOutcome.Completed(startedAt, _clock.GetUtcNow()), electricityConsumed);
        }

        void OnInterrupted(WasherProgram p)
        {
            var electricityConsumed = CalculateElectricityConsumed(readingBefore, meter.Read());
            Add(washer.Id, p, new CycleOutcome.Interrupted(startedAt, _clock.GetUtcNow()), electricityConsumed);
        }
    }

    public void Attach(Dryer dryer)
    {
        DateTimeOffset startedAt = default;

        dryer.ProgramStarted      += OnStarted;
        dryer.ProgramCompleted    += OnCompleted;
        dryer.ProgramInterrupted  += OnInterrupted;

        void OnStarted(DryerProgram p)     => startedAt = _clock.GetUtcNow();

        void OnCompleted(DryerProgram p)   => 
            Add(dryer.Id, p, new CycleOutcome.Completed(startedAt, _clock.GetUtcNow()));

        void OnInterrupted(DryerProgram p) => 
            Add(dryer.Id, p, new CycleOutcome.Interrupted(startedAt, _clock.GetUtcNow()));
    }

    private void Add(Guid washerId, WasherProgram program, CycleOutcome outcome, Kwh electricityConsumed)
    {
        var entry = new JournalEntry.Washer(washerId, program, outcome, electricityConsumed);
        _entries.Add(entry);
    }

    private void Add(Guid dryerId, DryerProgram program, CycleOutcome outcome)
    {
        var entry = new JournalEntry.Dryer(dryerId, program, outcome);
        _entries.Add(entry);
    }

    private Kwh CalculateElectricityConsumed(MeterReading before, MeterReading after) => after.Total - before.Total;
}