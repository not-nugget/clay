using System.Runtime.InteropServices;

namespace Clay.Types.Element;

/// <summary>Size of an element, represented as either a minimum or a maximum (<see cref="MinMax"/>),
/// <em>or</em> a percentage relative to its container (<see cref="Percent"/>)</summary>
[StructLayout(LayoutKind.Explicit)]
public struct Size
{
    [field: FieldOffset(0)]
    public SizingMinMax MinMax { get; set; }

    [field: FieldOffset(0)]
    public float Percent { get; set; }

    public static implicit operator Size(SizingMinMax   minMax) => new Size { MinMax = minMax };
    public static implicit operator Size((float, float) tuple)  => new Size { MinMax = tuple };

    public static implicit operator Size(float percent) => new Size { Percent = percent };
}