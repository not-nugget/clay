namespace Clay.Types.Element;

/// <summary>Specifies the radius of the edges of a rectangular element</summary>
public struct CornerRadius
{
    public  float TopLeft     { get; set; }
    private float TopRight    { get; set; }
    private float BottomLeft  { get; set; }
    private float BottomRight { get; set; }

    public static implicit operator CornerRadius(float                        radii) => new CornerRadius { TopLeft = radii, TopRight       = radii, BottomLeft       = radii, BottomRight       = radii };
    public static implicit operator CornerRadius((float, float, float, float) tuple) => new CornerRadius { TopLeft = tuple.Item1, TopRight = tuple.Item2, BottomLeft = tuple.Item3, BottomRight = tuple.Item4 };
}