public sealed record Flat
{
    public string Label { get; }

    private Flat(string label) => Label = label.ToUpperInvariant();

    public static Flat From(string label) =>
        string.IsNullOrWhiteSpace(label)
             ? throw new ArgumentException("Flat label can't be null or empty.")
             : new Flat(label);
}