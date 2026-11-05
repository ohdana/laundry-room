public class Narrator
{
    private readonly TimeProvider _clock;

    public Narrator(TimeProvider clock)
    {
        _clock = clock;
    }

    public void WatchRoom(Room room)
    {
        room.PowerOn += () => Say(room.Label, $"Card in ({room.CardOwner?.Label}), power on");
        room.PowerOff += () => Say(room.Label, $"Card out, power off");
    }

    public void WatchWasher(Washer washer, string room)
    {
        washer.ProgramStarted += program => Say(room, $"Washer started: {program}");
        washer.ProgramCompleted += program => Say(room, $"Washer completed: {program}");
        washer.ProgramInterrupted += program => Say(room, $"Washer interrupted: {program}");
    }

    public void WatchDryer(Dryer dryer, string room)
    {
        dryer.ProgramStarted += program => Say(room, $"Dryer started: {program}");
        dryer.ProgramCompleted += program => Say(room, $"Dryer completed: {program}");
        dryer.ProgramInterrupted += program => Say(room, $"Dryer interrupted: {program}");
    }

    public void WatchFan(Fan fan, string room)
    {
        fan.Started += program => Say(room, $"Fan started: {program}");
        fan.Stopped += program => Say(room, $"Fan stopped: {program}");
    }

    private void Say(string room, string text)
    {
        var time = _clock.GetUtcNow().ToString("HH:mm");
        Console.WriteLine($"{time} {room, -6} {text}");
    }
}