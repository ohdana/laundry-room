public class SimulatedMeter : IElectricityMeter
{
    private readonly TimeProvider _clock;
    private readonly decimal _ratedKw;
    private decimal _accumulatedKwh;
    private DateTimeOffset? _runningSince;

    public SimulatedMeter(TimeProvider clock, decimal ratedKw)
    {
        _clock = clock;
        _ratedKw = ratedKw;
    }

    public void Start()
    {
        if (_runningSince != null) return;
        _runningSince = _clock.GetUtcNow();
    }

    public void Stop()
    {
        _accumulatedKwh = CurrentKwh();
        _runningSince = null;
    }

    public MeterReading Read() => MeterReading.From(Kwh.From(CurrentKwh()));

    private decimal CurrentKwh()
    {
        if (_runningSince == null) return;
        var elapsed = _clock.GetUtcNow();
        var hours = (decimal)elapsed.Ticks / TimeSpan.TicksPerHour;
        return _accumulatedKwh + _ratedKw * hours;
    }
}