using System.Runtime.InteropServices;

namespace Clay.Types.Element;

public struct ScrollElementConfig : IElementConfig
{
    [field: MarshalAs(UnmanagedType.Bool)]
    public bool Horizontal { get; set; }

    [field: MarshalAs(UnmanagedType.Bool)]
    public bool Vertical { get; set; }
}