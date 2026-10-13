public class Washer
{
    public Guid Id { get; }
    public bool IsOn { get; private set; }
    public WasherState State { get; private set; } = new WasherState.Idle();

    private readonly IPowerSource _powerSource;

    public Washer(Guid id, IPowerSource powerSource)
    {
        if (id == Guid.Empty) throw new ArgumentException("Washer id can't be empty.", nameof(id));
        if (powerSource == null) throw new ArgumentNullException(nameof(powerSource));

        Id = id;
        _powerSource = powerSource;
        _powerSource.PowerOff += OnPowerOff;
    }

    private void OnPowerOff() => SwitchOff();

    public void SwitchOn()
    {
        if (!_powerSource.IsPowerOn) return;
        IsOn = true;
    }

    public void SwitchOff()
    {
        StopProgram();
        IsOn = false;
    }

    public void StartProgram(WasherProgram program)
    {
        if (!_powerSource.IsPowerOn) throw new InvalidOperationException("The power is off. Can't start the washer without power.");
        if (!IsOn) throw new InvalidOperationException("The washer is off. To start a program, switch it on.");
        if (State is WasherState.Running) throw new InvalidOperationException("To start a new program, stop the current program first.");
        
        State = new WasherState.Running(program);
    }

    private void StopProgram() => State = new WasherState.Idle();
}