public interface IPowerSource
{
    event Action? PowerOff;
    bool IsPowerOn { get; }
}