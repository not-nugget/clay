namespace Clay.Types.Element;

/// <summary>Controls sizing along an element's axis</summary>
public struct SizingAxis
{
    public Size       Size { get; set; }
    public SizingType Type { get; set; }

    public static implicit operator SizingAxis(float percent) => new SizingAxis
    {
        Size = new Size { Percent = percent },
        Type = SizingType.Percent,
    };

    public static implicit operator SizingAxis((float, float) tuple) => new SizingAxis
    {
        Size = new Size { MinMax = tuple },
        Type = SizingType.Fixed,
    };

    public static implicit operator SizingAxis(SizingType type) => new SizingAxis { Type = type, };
}