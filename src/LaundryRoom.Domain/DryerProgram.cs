public enum DryerProgram
{
    Cold,
    Normal,
    Hot
}

public static class DryerProgramExtensions
{
    public static TimeSpan Duration(this DryerProgram program) => program switch
    {
        DryerProgram.Cold   => TimeSpan.FromMinutes(240),
        DryerProgram.Normal => TimeSpan.FromMinutes(210),
        DryerProgram.Hot    => TimeSpan.FromMinutes(180),
        _                   => throw new ArgumentOutOfRangeException(nameof(program), program, null)
    };
}