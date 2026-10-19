public class Dryer
{
    public Guid Id { get; }
    public bool IsOn { get; private set; }
    public DryerState State { get; private set; } = new DryerState.Idle();
    public event Action<DryerProgram>? ProgramStarted;
    public event Action<DryerProgram>? ProgramCompleted;
    public event Action<DryerProgram>? ProgramInterrupted;

    private readonly IPowerSource _powerSource;
    private readonly TimeProvider _clock;
    private ITimer? _timer;

    public Dryer(Guid id, IPowerSource powerSource, TimeProvider clock)
    {
        if (id == Guid.Empty) throw new ArgumentException("Dryer id can't be empty.", nameof(id));
        if (powerSource == null) throw new ArgumentNullException(nameof(powerSource));

        Id = id;
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
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
        var program = State is DryerState.Running running ? running.Program : (DryerProgram?)null;
        StopProgram();
        IsOn = false;
        if (program != null) ProgramInterrupted?.Invoke(program.Value);
    }

    public void StartProgram(DryerProgram program)
    {
        if (!_powerSource.IsPowerOn) throw new InvalidOperationException("The power is off. Can't start the dryer without power.");
        if (!IsOn) throw new InvalidOperationException("The dryer is off. To start a program, switch it on.");
        if (State is DryerState.Running) throw new InvalidOperationException("To start a new program, stop the current program first.");
        
        State = new DryerState.Running(program);
        _timer = _clock.CreateTimer(_ => OnCycleElapsed(program), null, program.Duration(), Timeout.InfiniteTimeSpan);
        ProgramStarted?.Invoke(program);
    }

    private void StopProgram()
    {
        _timer?.Dispose();
        State = new DryerState.Idle();
    }

    private void OnCycleElapsed(DryerProgram program)
    {
        StopProgram();
        ProgramCompleted?.Invoke(program);
    }
}