using System.Runtime.InteropServices;

namespace Clay.Types.Internal.Interop;

/// <summary>Controls the lifetime of CLR strings that are marshaled into unmanaged memory</summary>
internal sealed class StringBufferHandle : SafeHandle
{
    /// <summary>String value of the handle</summary>
    internal string StringValue { get; }

    public StringBufferHandle(string strVal) : base(IntPtr.Zero, true)
    {
        StringValue = strVal;

        // We only need to marshal a non-null string
        if (!string.IsNullOrEmpty(strVal))
            handle = Marshal.StringToHGlobalAuto(StringValue);
    }

    protected override bool ReleaseHandle()
    {
        // No release is required if null or an empty string was marshalled
        if (string.IsNullOrEmpty(StringValue))
            return true;

        try
        {
            Marshal.FreeHGlobal(handle);
            handle = IntPtr.Zero;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public override bool IsInvalid
        => handle == IntPtr.Zero;
}