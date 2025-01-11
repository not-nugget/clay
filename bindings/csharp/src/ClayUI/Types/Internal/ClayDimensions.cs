namespace Clay.Types.Internal;

/// <summary>Dimensions of the Clay viewport</summary>
public struct ClayDimensions
{
    public float Width  { get; set; }
    public float Height { get; set; }

    public static implicit operator ClayDimensions((float, float) tuple) => new ClayDimensions { Width = tuple.Item1, Height = tuple.Item2 };
}