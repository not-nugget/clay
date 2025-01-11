namespace Clay.Types.Element;

/// <summary>Controls the sizing of an element</summary>
public struct Sizing
{
    public SizingAxis Width  { get; set; }
    public SizingAxis Height { get; set; }

    public static implicit operator Sizing((SizingAxis, SizingAxis) tuple) => new Sizing
    {
        Width  = tuple.Item1,
        Height = tuple.Item2,
    };
}