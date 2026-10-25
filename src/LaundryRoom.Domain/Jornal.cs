public class Journal
{
    public IReadOnlyList<JournalEntry> Entries => _entries.AsReadOnly();

    private readonly TimeProvider _clock;
    private readonly List<JournalEntry> _entries = new();

    public Journal(TimeProvider clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public void Attach(Washer washer, IElectricityMeter meter, Room room)
    {
        DateTimeOffset startedAt = default;
        MeterReading? readingBefore = null;
        string roomLabel = string.Empty;
        Flat? cardOwner = null;

        washer.ProgramStarted      += OnStarted;
        washer.ProgramCompleted    += OnCompleted;
        washer.ProgramInterrupted  += OnInterrupted;

        void OnStarted(WasherProgram p)
        {
            startedAt = _clock.GetUtcNow();
            readingBefore = meter.Read();
            roomLabel = room.Label;
            cardOwner = room.CardOwner;
        }

        void OnCompleted(WasherProgram p)
        {
            var electricityConsumed = CalculateElectricityConsumed(readingBefore, meter.Read());
            Add(washer.Id, roomLabel, cardOwner, p, new CycleOutcome.Completed(startedAt, _clock.GetUtcNow()), electricityConsumed);
        }

        void OnInterrupted(WasherProgram p)
        {
            var electricityConsumed = CalculateElectricityConsumed(readingBefore, meter.Read());
            Add(washer.Id, roomLabel, cardOwner, p, new CycleOutcome.Interrupted(startedAt, _clock.GetUtcNow()), electricityConsumed);
        }
    }

    public void Attach(Dryer dryer, IElectricityMeter meter)
    {
        DateTimeOffset startedAt = default;
        MeterReading? readingBefore = null;

        dryer.ProgramStarted      += OnStarted;
        dryer.ProgramCompleted    += OnCompleted;
        dryer.ProgramInterrupted  += OnInterrupted;

        void OnStarted(DryerProgram p)
        {
            startedAt = _clock.GetUtcNow();
            readingBefore = meter.Read();
        }

        void OnCompleted(DryerProgram p)
        {
            var electricityConsumed = CalculateElectricityConsumed(readingBefore, meter.Read());
            Add(dryer.Id, p, new CycleOutcome.Completed(startedAt, _clock.GetUtcNow()), electricityConsumed);
        }

        void OnInterrupted(DryerProgram p)
        {
            var electricityConsumed = CalculateElectricityConsumed(readingBefore, meter.Read());
            Add(dryer.Id, p, new CycleOutcome.Interrupted(startedAt, _clock.GetUtcNow()), electricityConsumed);
        }
    }

    private void Add(Guid washerId, string roomLabel, Flat cardOwner, WasherProgram program, CycleOutcome outcome, Kwh electricityConsumed)
    {
        var entry = new JournalEntry.Washer(washerId, roomLabel, cardOwner, program, outcome, electricityConsumed);
        _entries.Add(entry);
    }

    private void Add(Guid dryerId, DryerProgram program, CycleOutcome outcome, Kwh electricityConsumed)
    {
        var entry = new JournalEntry.Dryer(dryerId, program, outcome, electricityConsumed);
        _entries.Add(entry);
    }

    private Kwh CalculateElectricityConsumed(MeterReading before, MeterReading after) => after.Total - before.Total;
}