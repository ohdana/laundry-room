public class Fan
{
    public Guid Id { get; }
    public bool IsOn => State != new FanState.Off();
    public FanState State { get; private set; }

    private readonly IPowerSource _powerSource;
    private FanProgram _selectedProgram;
    private FanState _lastRecordedState;

    public Fan(Guid id, IPowerSource powerSource)
    {
        if (id == Guid.Empty) throw new ArgumentException("Fan id can't be empty.", nameof(id));
        if (powerSource == null) throw new ArgumentNullException(nameof(powerSource));

        Id = id;
        _powerSource = powerSource;
        State = new FanState.Off();
        _selectedProgram = FanProgram.Normal;
        _lastRecordedState = State;

        _powerSource.PowerOff += OnPowerOff;
        _powerSource.PowerOn += OnPowerOn;
    }

    public void SwitchOn() => State = new FanState.Running(_selectedProgram);

    public void SwitchOff() => State = new FanState.Off();

    public void SelectProgram(FanProgram program)
    {
        _selectedProgram = program;
        State = new FanState.Running(_selectedProgram);
    }

    private void OnPowerOn()
    {
        if (State is not FanState.Off) return;
        if (_lastRecordedState is FanState.Off) return;
        State = _lastRecordedState;
    }

    private void OnPowerOff()
    {
        if (State is FanState.Off) return;
        _lastRecordedState = State;
        State = new FanState.Off();
    }
}