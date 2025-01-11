namespace Clay.Types.Internal;

/// <summary>Unmanaged Clay memory arena</summary>
internal struct ClayArena
{
    #pragma warning disable CS0169 // Field is never used - Used in native code by Clay
    private UIntPtr _nextAllocation;
    private int     _capacity;
    #pragma warning restore CS0169 // Field is never used

    internal IntPtr Memory { get; private set; }
}