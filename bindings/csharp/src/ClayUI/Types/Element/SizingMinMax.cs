namespace Clay.Types.Element;

/// <summary>Represents the min and max values of a sizing constraint</summary>
public struct SizingMinMax(float min, float max)
{
    public float Min { get; set; } = min;
    public float Max { get; set; } = max;

    public static implicit operator SizingMinMax((float, float) tuple) => new SizingMinMax(tuple.Item1, tuple.Item2);
}