public abstract record CycleOutcome
{
    private CycleOutcome() { }
    public sealed record Completed : CycleOutcome
    {
        public DateTimeOffset StartedAt { get; }
        public DateTimeOffset CompletedAt { get; }

        public Completed(DateTimeOffset startedAt, DateTimeOffset completedAt)
        {
            if (completedAt <= startedAt)
                throw new ArgumentException("CompletedAt can't be before StartedAt", 
                    nameof(completedAt));
                    
            StartedAt = startedAt;
            CompletedAt = completedAt;
        }
    }
    
    public sealed record Interrupted : CycleOutcome
    {
        public DateTimeOffset StartedAt { get; }
        public DateTimeOffset InterruptedAt { get; }

        public Interrupted(DateTimeOffset startedAt, DateTimeOffset interruptedAt)
        {
            if (interruptedAt < startedAt)
                throw new ArgumentException("InterruptedAt can't be before StartedAt", 
                    nameof(interruptedAt));
                    
            StartedAt = startedAt;
            InterruptedAt = interruptedAt;
        }
    }
}