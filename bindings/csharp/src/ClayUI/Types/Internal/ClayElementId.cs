namespace Clay.Types.Internal;

/// <summary>Clay_ElementId Interop structure</summary>
internal struct ClayElementId
{
    #pragma warning disable CS0649 // Field is never assigned to, and will always have its default value - Field is assigned in unmanaged code
    internal uint       Id;
    internal uint       Offset;
    internal uint       BaseId;
    internal ClayString StringId;
    #pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
}