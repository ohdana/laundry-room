public enum WasherProgram
{
    Quick,
    Delicate,
    Cotton60
}

public static class WasherProgramExtensions
{
    public static TimeSpan Duration(this WasherProgram program) => program switch
    {
        WasherProgram.Quick    => TimeSpan.FromMinutes(30),
        WasherProgram.Delicate => TimeSpan.FromMinutes(45),
        WasherProgram.Cotton60 => TimeSpan.FromMinutes(55),
        _                      => throw new ArgumentOutOfRangeException(nameof(program), program, null);
    };
}