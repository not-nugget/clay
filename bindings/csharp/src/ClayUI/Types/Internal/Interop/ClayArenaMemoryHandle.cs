using System.Runtime.InteropServices;

namespace Clay.Types.Internal.Interop;

/// <summary>Controls the unmanaged lifetime of the <see cref="ClayArena"/> memory pointer</summary>
internal sealed class ClayArenaMemoryHandle : SafeHandle
{
    public override bool IsInvalid
        => handle == IntPtr.Zero;

    public ClayArenaMemoryHandle(uint byteCount) : base(IntPtr.Zero, true) 
        => handle = Marshal.AllocHGlobal((int)byteCount);

    protected override bool ReleaseHandle()
    {
        try
        {
            Marshal.FreeHGlobal(handle);
            return true;
        }
        catch
        {
            return false;
        }
    }
}