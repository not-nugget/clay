namespace Clay.Types.Element;

/// <summary>Controls an element's padding</summary>
public struct Padding
{
    public ushort X { get; set; }
    public ushort Y { get; set; }

    public static implicit operator Padding((ushort, ushort) tuple) => new Padding
    {
        X = tuple.Item1,
        Y = tuple.Item2,
    };
}