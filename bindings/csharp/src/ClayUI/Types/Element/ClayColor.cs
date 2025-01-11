namespace Clay.Types.Element;

/// <summary>Represents a 4-channel color, stored in RGBA format</summary>
public struct ClayColor
{
    /// <summary>Red channel</summary>
    public float R { get; set; }

    /// <summary>Green channel</summary>
    public float G { get; set; }

    /// <summary>Blue channel</summary>
    public float B { get; set; }

    /// <summary>Alpha channel</summary>
    public float A { get; set; }

    public static implicit operator ClayColor(float                        spread) => new ClayColor { R = spread, G      = spread, B      = spread, A      = 255f };
    public static implicit operator ClayColor((float, float, float)        tuple)  => new ClayColor { R = tuple.Item1, G = tuple.Item2, B = tuple.Item3, A = 255f };
    public static implicit operator ClayColor((float, float, float, float) tuple)  => new ClayColor { R = tuple.Item1, G = tuple.Item2, B = tuple.Item3, A = tuple.Item4 };
}