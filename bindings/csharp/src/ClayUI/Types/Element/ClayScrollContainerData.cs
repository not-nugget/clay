using System.Runtime.InteropServices;

using Clay.Types.Internal;

namespace Clay.Types.Element;

public struct ClayScrollContainerData
{
    public  ClayVector2 ScrollPosition => Marshal.PtrToStructure<ClayVector2>(_scrollPosition);
    private IntPtr      _scrollPosition;

    public ClayDimensions      ScrollContainerDimensions { get; set; }
    public ClayDimensions      ContentDimensions         { get; set; }
    public ScrollElementConfig Config                    { get; set; }

    [field: MarshalAs(UnmanagedType.Bool)]
    public bool Found { get; set; }
}