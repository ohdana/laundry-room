public abstract record FanState
{
    private FanState() { }
    public sealed record Off : FanState;
    public sealed record Idle : FanState;
    public sealed record Running(FanProgram Program) : FanState;
}