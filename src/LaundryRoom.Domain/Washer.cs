public class Washer
{
    public Guid Id { get; }
    public bool IsOn { get; private set; }
    public WasherState State { get; private set; } = new WasherState.Idle();
    public event Action<WasherProgram>? ProgramStarted;
    public event Action<WasherProgram>? ProgramCompleted;

    private readonly IPowerSource _powerSource;
    private readonly TimeProvider _clock;
    private ITimer? _timer;

    public Washer(Guid id, IPowerSource powerSource, TimeProvider clock)
    {
        if (id == Guid.Empty) throw new ArgumentException("Washer id can't be empty.", nameof(id));
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
        StopProgram();
        IsOn = false;
    }

    public void StartProgram(WasherProgram program)
    {
        if (!_powerSource.IsPowerOn) throw new InvalidOperationException("The power is off. Can't start the washer without power.");
        if (!IsOn) throw new InvalidOperationException("The washer is off. To start a program, switch it on.");
        if (State is WasherState.Running) throw new InvalidOperationException("To start a new program, stop the current program first.");
        
        State = new WasherState.Running(program);
        _timer = _clock.CreateTimer(_ => OnCycleElapsed(program), null, program.Duration(), Timeout.InfiniteTimeSpan);
        ProgramStarted?.Invoke(program);
    }

    private void StopProgram()
    {
        _timer?.Dispose();
        State = new WasherState.Idle();
    }

    private void OnCycleElapsed(WasherProgram program)
    {
        StopProgram();
        ProgramCompleted?.Invoke(program);
    }
}