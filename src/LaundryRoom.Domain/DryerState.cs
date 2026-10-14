public abstract record DryerState
{
    private DryerState() { }
    public sealed record Idle : DryerState;
    public sealed record Running(DryerProgram Program) : DryerState;
}