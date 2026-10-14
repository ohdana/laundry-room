public class Dryer
{
    public Guid Id { get; }
    public bool IsOn { get; private set; }
    public DryerState State { get; private set; } = new DryerState.Idle();

    private readonly IPowerSource _powerSource;

    public Dryer(Guid id, IPowerSource powerSource)
    {
        if (id == Guid.Empty) throw new ArgumentException("Dryer id can't be empty.", nameof(id));
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

    public void StartProgram(DryerProgram program)
    {
        if (!_powerSource.IsPowerOn) throw new InvalidOperationException("The power is off. Can't start the dryer without power.");
        if (!IsOn) throw new InvalidOperationException("The dryer is off. To start a program, switch it on.");
        if (State is DryerState.Running) throw new InvalidOperationException("To start a new program, stop the current program first.");
        
        State = new DryerState.Running(program);
    }

    private void StopProgram() => State = new DryerState.Idle();
}