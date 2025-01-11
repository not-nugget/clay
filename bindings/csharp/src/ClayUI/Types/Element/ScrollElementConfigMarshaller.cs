using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;

namespace Clay.Types.Element;

[CustomMarshaller(typeof(ScrollElementConfig), MarshalMode.ManagedToUnmanagedIn, typeof(ScrollElementConfigMarshaller))]
public static unsafe class ScrollElementConfigMarshaller
{
    private readonly ref struct ScrollElementConfigInterop(byte bHorizontal, byte bVertical)
    {
        private readonly byte _bHorizontal = bHorizontal;
        private readonly byte _bVertical   = bVertical;
    }

    public static nint ConvertToUnmanaged(ScrollElementConfig managed)
    {
        var bHorizontal = (byte)(managed.Horizontal ? 1 : 0);
        var bVertical   = (byte)(managed.Vertical ? 1 : 0);
        var interop     = new ScrollElementConfigInterop(bHorizontal, bVertical);
        return (nint)Unsafe.AsPointer(ref interop);
    }
}