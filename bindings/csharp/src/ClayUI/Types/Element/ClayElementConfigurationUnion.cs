namespace Clay.Types.Element;

/// <summary>Required for interoperability. Should not be user facing</summary>
internal struct ClayElementConfigurationUnion
{
    // NOTE: there is no need to have all the different pointers, the C union
    // is just a wrapper around single pointer with more type definition information
    public IntPtr ConfigPointer { get; init; }

    public static explicit operator ClayElementConfigurationUnion(IntPtr pointer) => new ClayElementConfigurationUnion { ConfigPointer = pointer };
}