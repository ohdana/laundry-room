public interface IPowerSource
{
    event Action? PowerOff;
    event Action? PowerOn;
    bool IsPowerOn { get; }
}