namespace Clay.Types.Internal;

internal struct ClayStringArray
{
    //TODO is this type needed for interop? 
    //TODO Clay_StringArray is a private type and therefore should not ever be user-accessible
    private int    capacity;
    private int    length;
    private IntPtr array; //TODO SafeHandle, or unsafe ClayString*, or alternative layout?
}