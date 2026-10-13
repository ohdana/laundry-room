/*public enum WasherState
{
    Idle,
    Running
}*/

public abstract record WasherState
{
    private WasherState() { }
    public sealed record Idle : WasherState;
    public sealed record Running(WasherProgram Program) : WasherState;
}