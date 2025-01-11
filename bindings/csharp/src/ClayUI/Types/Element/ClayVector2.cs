namespace Clay.Types.Element;

public struct ClayVector2
{
    //TODO if System.Numerics.Vector2 is blittable (it should be, right?) id rather use that for interop
    public float X { get; set; }
    public float Y { get; set; }
}