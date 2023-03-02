namespace GameSharp;

internal sealed class Operator
{
    public required byte Precedence { get; init; }
    public required Associativities Associativity { get; init; }

    internal enum Associativities : byte
    {
        Associative,
        Left,
        Right,
        None
    }
}
