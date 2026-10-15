public enum WasherProgram
{
    Quick,
    Delicate,
    Cotton60
}

public class WasherSettings
{
    public static Dictionary<WasherProgram, int> ProgramDurationMap { get; } = new()
    {
        [WasherProgram.Quick] = 30,
        [WasherProgram.Delicate] = 45,
        [WasherProgram.Cotton60] = 55
    };
}