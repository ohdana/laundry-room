public class Fan
{
    public Guid Id { get; }
    public bool IsOn => State != new FanState.Off();
    public FanState State { get; private set; } = new FanState.Idle();

    private readonly IPowerSource _powerSource;
    private FanProgram _selectedProgram;
    private FanState _lastRecordedState;

    public Fan(Guid id, IPowerSource powerSource)
    {
        if (id == Guid.Empty) throw new ArgumentException("Fan id can't be empty.", nameof(id));
        if (powerSource == null) throw new ArgumentNullException(nameof(powerSource));

        Id = id;
        _powerSource = powerSource;
        State = _powerSource.IsPowerOn ? new FanState.Idle() : new FanState.Off();
        _selectedProgram = FanProgram.Normal;
        _lastRecordedState = new FanState.Idle();

        _powerSource.PowerOff += OnPowerOff;
        _powerSource.PowerOn += OnPowerOn;
    }

    public void Start()
    {
        if (State is FanState.Off) throw new InvalidOperationException("The power is off. Can't start the fan without power.");
        if (State is FanState.Running) throw new InvalidOperationException("To start a new program, stop the current program first.");
        
        State = new FanState.Running(_selectedProgram);
    }

    public void Stop()
    {
        if (State is not FanState.Off) State = new FanState.Idle();
    }

    public void SelectProgram(FanProgram program) => _selectedProgram = program;

    private void OnPowerOn()
    {
        if (State is not FanState.Off) return;
        State = _lastRecordedState;
    }

    private void OnPowerOff()
    {
        if (State is FanState.Off) return;
        _lastRecordedState = State;
        State = new FanState.Off();
    }
}