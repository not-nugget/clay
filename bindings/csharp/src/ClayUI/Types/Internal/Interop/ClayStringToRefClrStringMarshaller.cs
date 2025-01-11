using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Clay.Types.Internal.Interop;

/// <summary>Handles the marshalling of a <see cref="ClayString"/> pointer</summary>
[CustomMarshaller(typeof(string), MarshalMode.UnmanagedToManagedRef, typeof(ClayStringToRefClrStringMarshaller))]
internal static unsafe class ClayStringToRefClrStringMarshaller
{
    public static string ConvertToManaged(nint unmanaged)
    {
        var clayString = Marshal.PtrToStructure<ClayString>(unmanaged);
        return Marshal.PtrToStringUTF8(clayString.Chars, clayString.Length);
    }

    //TODO String safe handle? Not sure if i want it freed within this method's scope, though
    public static nint ConvertToUnmanaged(string managed)
    {
        var clayStr = new ClayString
        {
            Length = managed.Length,
            Chars  = Marshal.StringToCoTaskMemUTF8(managed),
        };
        return new IntPtr(Unsafe.AsPointer(ref clayStr));
    }
}